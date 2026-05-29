using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Nexus.Core.DTOs;
using Nexus.Core.Interfaces;

namespace Nexus.Scraper.Storyblocks;

public sealed class StoryblocksScraper : IStoryblocksScraper, IAsyncDisposable
{
    private readonly StoryblocksScraperOptions _options;
    private readonly ILogger<StoryblocksScraper> _logger;
    private readonly Random _random = new();
    private IPlaywright? _playwright;

    public StoryblocksScraper(
        IOptions<StoryblocksScraperOptions> options,
        ILogger<StoryblocksScraper> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> EnsureAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        await using var session = await CreateSessionAsync(requireSearchReady: true, cancellationToken);
        return session.Page is not null;
    }

    public async Task<IReadOnlyList<ScrapedMediaResult>> SearchAndDownloadAsync(
        string query,
        int maxDownloads = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        if (maxDownloads < 1)
            throw new ArgumentOutOfRangeException(nameof(maxDownloads));

        var results = new List<ScrapedMediaResult>();
        await using var session = await CreateSessionAsync(requireSearchReady: true, cancellationToken);

        // === Single-page contract ===
        // The entire pipeline below operates on `page` — the page instance
        // created during CreateSessionAsync — and never re-creates it.
        var page = session.Page
            ?? throw new InvalidOperationException(
                "CreateSessionAsync returned without an active page.");
        if (page.IsClosed)
            throw new InvalidOperationException(
                "Active page is already closed before search; session state was not applied.");

        // Install the popup/extra-page guards NOW — after auth has finished.
        // During CreateSessionAsync the user may have been mid-OAuth and the
        // Google sign-in popup needs to stay alive; those guards used to be
        // there and killed the OAuth popup. We delay them to the scraping
        // phase, where any new page/tab IS suspicious (related-clip link,
        // signup nag, download-in-new-tab).
        InstallScrapingPopupGuards(session.Context, page);

        await RunSearchWithFiltersAsync(page, query, cancellationToken);

        var cards = page.Locator(StoryblocksSelectors.VideoCard);
        var count = await cards.CountAsync();
        if (count == 0)
        {
            _logger.LogWarning("No video cards found for query: {Query}", query);
            return results;
        }

        var downloadDir = Path.GetFullPath(_options.DownloadDirectory);
        Directory.CreateDirectory(downloadDir);

        var toDownload = Math.Min(maxDownloads, count);

        // === Simple random card selection ===
        // Storyblocks renders the thumbnail as a <video> element, NOT <img>.
        // Any pre-download child-locator read on a card (`Locator("img").First.
        // GetAttributeAsync(...)`, etc.) therefore hits Playwright's default
        // 30-second actionability timeout PER CARD, which is what killed the
        // previous scoring pass. We avoid every such read here:
        //   - The candidate pool is just "the top 5 grid positions" (or
        //     fewer if the query returned a smaller result set). Storyblocks
        //     already ranks by relevance so the first 5 are quality picks.
        //   - We pick a fresh random index from that pool per slot via
        //     `_random.Next(0, Math.Min(count, 5))`, tracking used indices
        //     in a HashSet so the same clip is never downloaded twice in a
        //     single call.
        //   - All actual DOM interaction is deferred to DownloadFromCardAsync,
        //     which uses the F8-mapped `[data-cy='download-button']` overlay.
        var randomPoolSize = Math.Min(count, 5);
        var usedIndices = new HashSet<int>(randomPoolSize);

        // Track the last per-card failure so that if EVERY attempt fails we can
        // attach it as InnerException for diagnostics. We deliberately keep the
        // per-card try/catch (partial success is valuable — if 1/3 clips download
        // we still want to return the one that worked) but escalate the all-fail
        // case to a thrown exception so the API surfaces a 500 instead of [].
        Exception? lastError = null;

        for (var slot = 0; slot < toDownload; slot++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If the caller asked for more downloads than there are unique
            // slots in the pool (e.g. maxDownloads=10 but only 3 cards match),
            // stop here rather than infinite-looping in the dedup retry.
            if (usedIndices.Count >= randomPoolSize)
            {
                _logger.LogInformation(
                    "Exhausted random pool ({PoolSize} cards) after {Slot} download(s).",
                    randomPoolSize, slot);
                break;
            }

            int cardIndex;
            do
            {
                cardIndex = _random.Next(0, randomPoolSize);
            }
            while (!usedIndices.Add(cardIndex));

            var card = cards.Nth(cardIndex);

            try
            {
                var savedPath = await DownloadFromCardAsync(page, card, downloadDir, cancellationToken);
                // Read ONLY the card's own aria-label (a synchronous attribute
                // read on the matched element — no child lookup, no timeout
                // risk). The previous `?? img.alt` fallback was the second
                // source of 30s hangs and is now gone for good.
                var title = await card.GetAttributeAsync("aria-label");
                var tags = await ExtractTagsAsync(card, title);

                results.Add(new ScrapedMediaResult(savedPath, title, tags));

                _logger.LogInformation(
                    "Downloaded clip {Slot}/{Total} (grid pos {GridIndex}): {Path} (tags: {TagCount})",
                    slot + 1, toDownload, cardIndex, savedPath, tags.Count);
            }
            catch (Exception ex)
            {
                lastError = ex;
                _logger.LogError(ex, "Failed to download card at grid index {GridIndex}", cardIndex);
            }

            await HumanDelayAsync(cancellationToken);
        }

        if (toDownload > 0 && results.Count == 0)
        {
            // === TEMPORARY DEBUG INSTRUMENTATION — revert before production ===
            // Capture the full page so we can see exactly what the DOM looked
            // like when every download attempt failed. The screenshot is saved
            // to the API process's current working directory (typically the
            // solution root when launched via Nexus.CLI). We swallow any
            // screenshot error so it can never mask the original failure.
            // Track-tag: SCRAPER_SCREENSHOT_ON_FAIL
            const string screenshotPath = "error_screenshot.png";
            try
            {
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });
                _logger.LogWarning(
                    "All download attempts failed. Saved screenshot to {AbsolutePath}",
                    Path.GetFullPath(screenshotPath));
            }
            catch (Exception screenshotEx)
            {
                _logger.LogError(screenshotEx, "Could not save error screenshot.");
            }

            throw new InvalidOperationException(
                "Failed to download any clips. Check logs for selector timeouts.",
                lastError);
        }

        return results;
    }

    private async Task<BrowserSession> CreateSessionAsync(
        bool requireSearchReady,
        CancellationToken cancellationToken)
    {
        _playwright ??= await Playwright.CreateAsync();
        var statePath = Path.GetFullPath(_options.SessionStatePath);
        var hasState = SessionStatePersistence.Exists(statePath);

        // === TEMPORARY DEBUG OVERRIDE — revert before production ===
        // Forcing Headless = false (ignoring hasState) so the operator can
        // visually watch the bot hover and click the cards. Restore the
        // original `Headless = hasState` once download-button timeouts are
        // diagnosed. Track-tag: SCRAPER_HEADED_DEBUG
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Args = ["--disable-blink-features=AutomationControlled"]
        });

        // === Session state injection at CONTEXT CREATION ===
        // Storyblocks is a React SPA that keeps its authenticated JWT in
        // `localStorage`, NOT in HTTP cookies. The previous design called
        // `context.AddCookiesAsync(...)` AFTER context creation, which:
        //   1. Couldn't carry localStorage at all (the API only takes Cookie[]),
        //   2. Made the auth landing race the cookie injection on first nav.
        // Playwright's native StorageStatePath hands the file off to the
        // browser BEFORE the first request fires, AND restores localStorage /
        // sessionStorage simultaneously. So the very first GET on
        // storyblocks.com lands with a fully-rehydrated authenticated context.
        var contextOptions = new BrowserNewContextOptions
        {
            AcceptDownloads = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            Locale = "en-US",
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        };
        if (hasState)
        {
            contextOptions.StorageStatePath = statePath;
            _logger.LogInformation(
                "Loading session state (cookies + localStorage) from {StatePath}",
                statePath);
        }
        var context = await browser.NewContextAsync(contextOptions);

        var page = await context.NewPageAsync();

        // NOTE: Popup-killing guards are NOT installed here. The "Continue
        // with Google" sign-in flow legitimately opens an OAuth popup, and
        // killing that popup before the user can complete sign-in was making
        // manual authentication impossible. The guards now live inside
        // SearchAndDownloadAsync, where they only come online AFTER auth is
        // confirmed and we're about to start scraping — see
        // `InstallScrapingPopupGuards` below.

        await page.GotoAsync(_options.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 60_000
        });

        if (!hasState)
        {
            await WaitForManualGoogleLoginAsync(page, statePath, cancellationToken);
        }

        if (requireSearchReady)
            await WaitForVisibleSearchInputAsync(page, timeoutMs: 60_000);

        return new BrowserSession(browser, context, page);
    }

    // The Storyblocks layout ships a hidden mobile-nav input AND a visible
    // desktop input matching the same selector. `Page.WaitForSelectorAsync`
    // watches the first DOM match and would hang on the hidden mobile input,
    // so we instead build a Locator, filter to visible matches, take `.First`,
    // and wait for that explicitly. The selector itself also uses `:visible`,
    // making this filter belt-and-braces against future call sites.
    private static ILocator GetVisibleSearchInput(IPage page) =>
        page.Locator(StoryblocksSelectors.SearchInput)
            .Filter(new LocatorFilterOptions { Visible = true })
            .First;

    private static Task WaitForVisibleSearchInputAsync(IPage page, int timeoutMs) =>
        GetVisibleSearchInput(page).WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

    private async Task WaitForManualGoogleLoginAsync(
        IPage page,
        string statePath,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "No session state file found. Browser opened in HEADED mode — sign in with Google manually.");
        Console.WriteLine();
        Console.WriteLine("=== Storyblocks manual login ===");
        Console.WriteLine("1. Click \"Sign in with Google\" in the browser window.");
        Console.WriteLine("2. Complete OAuth in the browser.");
        Console.WriteLine("3. Wait until the Storyblocks account avatar is visible in the top-right.");
        Console.WriteLine($"   (timeout: {_options.ManualLoginTimeoutMinutes} minutes)");
        Console.WriteLine();

        var timeoutMs = _options.ManualLoginTimeoutMinutes * 60_000;
        await WaitForVisibleSearchInputAsync(page, timeoutMs);

        // === Authoritative logged-in sync point ===
        // The search bar is visible to LOGGED-OUT visitors too — so a
        // "search bar visible" check fires immediately on home-page load,
        // well before Google's OAuth callback chain (Google → Storyblocks
        // callback → home → premium-tier hydration) finishes writing the
        // full cookie jar. Persisting at that point gave us the infamous
        // "Read 1 session cookies" symptom: only `connect.sid` had landed.
        //
        // The Account avatar in the header is ONLY rendered once Storyblocks
        // has confirmed the user is authenticated, which means the redirect
        // chain has finished and every session cookie is in the jar. We
        // wait up to 2 minutes for it to appear (the user may need time to
        // complete 2FA, pick a Google account, accept consent, etc.).
        _logger.LogInformation(
            "Search input is visible — waiting for the Account icon to confirm full sign-in...");
        Console.WriteLine("Waiting for sign-in to complete (account icon)...");
        await page.WaitForSelectorAsync(
            StoryblocksSelectors.AccountIndicator,
            new PageWaitForSelectorOptions { Timeout = 120_000 });

        // React often finishes writing the premium-tier JWT to localStorage
        // ONE tick after the avatar paints. A 2-second buffer here means
        // the very next `context.StorageStateAsync(...)` snapshot captures
        // a fully-formed auth jar (cookies + localStorage + sessionStorage).
        await page.WaitForTimeoutAsync(2000);
        _logger.LogInformation("Account icon visible — sign-in confirmed.");

        // === Inline StorageState snapshot ===
        // We persist the snapshot RIGHT HERE rather than via a helper so
        // the auth call site is the single, obvious place where state is
        // written. Playwright's StorageStateAsync writes the file atomically
        // and includes cookies + localStorage + sessionStorage — the
        // architectural fix for the earlier cookie-only persistence which
        // dropped the React JWT on the floor.
        var statePathFull = Path.GetFullPath(statePath);
        var statePathDir = Path.GetDirectoryName(statePathFull);
        if (!string.IsNullOrEmpty(statePathDir))
            Directory.CreateDirectory(statePathDir);
        await page.Context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = statePathFull
        });
        _logger.LogInformation(
            "Authentication successful. State saved to {StatePath}", statePathFull);
        Console.WriteLine($"Session state (cookies + localStorage) saved to: {statePathFull}");

        // NOTE: we deliberately do NOT call `page.CloseAsync()` here.
        // CreateSessionAsync immediately uses the same `page` for
        // `WaitForVisibleSearchInputAsync(...)` once we return, and
        // closing it would throw. The whole context — including this
        // page — is disposed cleanly via BrowserSession.DisposeAsync at
        // the end of the public API call.
    }

    private async Task RunSearchWithFiltersAsync(IPage page, string query, CancellationToken cancellationToken)
    {
        var search = GetVisibleSearchInput(page);
        await search.ClickAsync();
        await HumanDelayAsync(cancellationToken);
        await search.FillAsync(query);
        await HumanDelayAsync(cancellationToken);

        var submit = page.Locator(StoryblocksSelectors.SearchSubmit)
            .Filter(new LocatorFilterOptions { Visible = true })
            .First;
        if (await submit.CountAsync() > 0 && await submit.IsVisibleAsync())
            await submit.ClickAsync();
        else
            await search.PressAsync("Enter");

        // Storyblocks is a heavy SPA: telemetry, prefetch, ad pixels and live
        // recommendation polling mean LoadState.NetworkIdle effectively never
        // fires and any wait on it hits its timeout. We instead wait for a
        // specific DOM signal that the search-results route has committed:
        // the "Filters" toggle button, which is part of the results-page
        // chrome and only appears once the SPA has rendered the results view.
        var filtersBtn = page.Locator(StoryblocksSelectors.FiltersToggle)
            .Filter(new LocatorFilterOptions { Visible = true })
            .First;
        await filtersBtn.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60_000
        });
        await HumanDelayAsync(cancellationToken);

        // === Conditional Filters drawer toggle ===
        // The filters drawer is a TOGGLE — clicking it when already open
        // COLLAPSES it, which means our subsequent checkbox clicks would land
        // on hidden inputs. The drawer's open state is reflected by the
        // `open-side-menu` class on its wrapper:
        //     <div class="videoblocks side-menu block open-side-menu">
        // We check for that class first and only click the toggle if the
        // drawer is currently closed. If it's already open (e.g. left open
        // from a previous run sharing the same browser context), we skip the
        // click entirely and proceed straight to the checkbox manipulation.
        var isPanelOpen = await page.Locator(".open-side-menu").CountAsync() > 0;
        if (isPanelOpen)
        {
            _logger.LogInformation("Filters drawer already open — skipping toggle click.");
        }
        else
        {
            await filtersBtn.ClickAsync();
            // Deterministic 750ms (NOT a HumanDelay) for the drawer's mount
            // animation. A selector-based wait isn't reliable here because the
            // drawer root mounts before its child inputs are bound.
            await page.WaitForTimeoutAsync(750);
        }

        await page.Locator(StoryblocksSelectors.FilterFootage).ClickAsync();
        await HumanDelayAsync(cancellationToken);
        await page.Locator(StoryblocksSelectors.FilterVertical).ClickAsync();
        await HumanDelayAsync(cancellationToken);
        await page.Locator(StoryblocksSelectors.Filter4K).ClickAsync();
        await HumanDelayAsync(cancellationToken);

        // Same rationale as above — NetworkIdle is unreliable on Storyblocks.
        // Once the three filter checkboxes have been toggled the page mutates
        // the URL and re-hydrates the grid; the unambiguous "we're ready to
        // download" signal is the presence of at least one visible video card.
        // The HumanDelays between filter clicks give the SPA time to coalesce
        // its filter state into one re-render request, so by the time we get
        // here we expect to be waiting on the re-hydrated grid, not the stale
        // pre-filter one.
        await page.Locator(StoryblocksSelectors.VideoCard)
            .Filter(new LocatorFilterOptions { Visible = true })
            .First
            .WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 60_000
            });
    }

    private async Task<string> DownloadFromCardAsync(
        IPage page,
        ILocator card,
        string downloadDir,
        CancellationToken cancellationToken)
    {
        // === Step A: Hover the CARD to materialize the overlay button.
        //
        // The Hover Catch-22: `[data-cy='download-button']` does NOT exist in
        // the DOM until the card receives a `mouseenter`. The previous version
        // of this code skipped the card hover and went straight to the button
        // — which made every interaction time out for 30 s waiting for a
        // ghost element. We now hover the card first to trigger Storyblocks'
        // React component to mount the overlay, then we can locate and click
        // the button.
        //
        // ScrollIntoViewIfNeededAsync on the CARD (not the button) is safe:
        // the card itself is always in the DOM after the grid renders.
        //
        // HoverAsync with Force=true means we bypass actionability checks —
        // even if the card surface is briefly covered by a tooltip / cookie
        // banner / hover-overlay from a sibling card, we still dispatch the
        // mouseenter. The card-wrapper anchor's navigation only fires on
        // CLICK, not HOVER, so hovering with Force is non-destructive.
        await card.ScrollIntoViewIfNeededAsync();
        await card.HoverAsync(new LocatorHoverOptions { Force = true });
        // 500 ms gives React one full paint cycle + a buffer to mount the
        // overlay's `data-cy="download-button"` div. The previous 300 ms
        // was sometimes shorter than the React state transition on a
        // freshly-scrolled card.
        await page.WaitForTimeoutAsync(500);

        // === Step B: NOW the button exists — locate and click.
        //
        // We deliberately re-resolve the locator AFTER the hover so the lazy
        // selector evaluation hits the freshly-mounted element. Force=true
        // on the click skips actionability checks (the button is briefly
        // opacity:0 during the React transition) AND, crucially, dispatches
        // the click as a synthetic event that bypasses the card-wrapper
        // anchor's bubbling — clicking the button never navigates the page.
        var overlayButton = card.Locator(StoryblocksSelectors.CardOverlayDownloadButton).First;
        await overlayButton.ClickAsync(new LocatorClickOptions { Force = true });

        // === Step C: Hard wait for the dropdown to hydrate.
        // The format-picker's children mount one render frame after the click
        // dispatches. Waiting on a selector here would be unreliable because the
        // dropdown root mounts first and its child buttons populate slightly
        // later — a selector-based wait can transiently match a stale stub.
        // A 1000ms fixed delay sits well inside our human-pacing budget and
        // gives the React tree time to finish the state transition.
        await page.WaitForTimeoutAsync(1000);

        // === Step D: Resolve the resolution-specific button by INNER TEXT.
        // We do NOT match on `title` — the per-clip codec/size suffix
        // ("4K MP4 (hevc) - 7.8 MB") means there is no stable literal to anchor
        // on. Instead we scope to BaseDownloadButton (the stable code-identifier
        // class `download-triggers`) and chain two `Filter(HasText = ...)`
        // narrowings: one for the resolution token, one for the codec. The pair
        // produces a unique match per card.
        //
        // Preference order is 4K → HD: the search filters already constrain
        // results to clips that ship a 4K master, but some Storyblocks clips
        // (notably older "Premium" catalogue items) only offer HD. Falling back
        // is cheaper than failing the whole loop iteration.
        var btn4K = card.Locator(StoryblocksSelectors.BaseDownloadButton)
            .Filter(new LocatorFilterOptions { HasText = "4K" })
            .Filter(new LocatorFilterOptions { HasText = "mp4" })
            .First;

        var btnHD = card.Locator(StoryblocksSelectors.BaseDownloadButton)
            .Filter(new LocatorFilterOptions { HasText = "HD" })
            .Filter(new LocatorFilterOptions { HasText = "mp4" })
            .First;

        ILocator targetButton;
        string chosenResolution;
        if (await btn4K.CountAsync() > 0)
        {
            targetButton = btn4K;
            chosenResolution = "4K MP4";
        }
        else if (await btnHD.CountAsync() > 0)
        {
            targetButton = btnHD;
            chosenResolution = "HD MP4";
        }
        else
        {
            throw new InvalidOperationException(
                "Card download dropdown contained neither '4K MP4' nor 'HD MP4' buttons. " +
                "The format picker may not have rendered, or the resolution-token markup changed.");
        }

        await targetButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000
        });
        _logger.LogInformation("Selected download format: {Resolution}", chosenResolution);

        // === Step E: Subscribe to the Playwright Download event, THEN click.
        // The subscription must happen BEFORE the click because Chromium's
        // download dispatch is synchronous from its side — a late subscribe
        // would miss the event entirely. We never poll the browser's UI tray;
        // the IDownload handle returned by WaitForDownloadAsync is the source
        // of truth for this transfer.
        var downloadTask = page.WaitForDownloadAsync(new PageWaitForDownloadOptions
        {
            Timeout = _options.DownloadTimeoutSeconds * 1000
        });

        await targetButton.ClickAsync();
        var download = await downloadTask;

        var suggested = download.SuggestedFilename;
        if (string.IsNullOrWhiteSpace(suggested) || !suggested.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            suggested = $"storyblocks_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.mp4";

        var targetPath = Path.Combine(downloadDir, suggested);
        await download.SaveAsAsync(targetPath);

        // === Flush barrier: ensure the payload is fully written to disk before
        // the loop moves on to the next card. SaveAsAsync above already waits
        // internally, but calling PathAsync gives us an explicit checkpoint
        // and surfaces a clear failure if Playwright reports the transfer was
        // aborted (e.g. browser closed mid-transfer, server connection reset).
        var playwrightTempPath = await download.PathAsync();
        if (string.IsNullOrEmpty(playwrightTempPath))
            throw new InvalidOperationException(
                "Playwright reports no download payload on disk — the transfer was aborted before completion.");

        if (!File.Exists(targetPath))
            throw new InvalidOperationException($"Download did not persist to disk: {targetPath}");

        return targetPath;
    }

    private static async Task<IReadOnlyList<string>> ExtractTagsAsync(ILocator card, string? title)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Strategy 1: any explicit chip/tag links inside the card (Storyblocks
        // renders "related keyword" pills as <a> tags inside the card hover overlay).
        try
        {
            var chipTexts = await card.Locator(StoryblocksSelectors.TagChip).AllInnerTextsAsync();
            foreach (var t in chipTexts)
                AddCleanedTag(tags, t);
        }
        catch
        {
            // Tag chips are best-effort — DOM shape changes, never fail download because of them.
        }

        // Strategy 2: parse the <img> alt attribute IF the card actually has
        // one. Storyblocks cards now render thumbnails as <video> so this
        // path is usually empty — we MUST short-circuit on CountAsync first,
        // because GetAttributeAsync would otherwise wait the default 30s
        // actionability timeout for a missing element (Playwright .NET
        // doesn't expose a "skip wait if absent" mode for this call). The
        // 1-second cap on the actual read is belt-and-braces against a slow
        // attribute hydrate in case the element is mid-mount.
        try
        {
            var imgLocator = card.Locator("img").First;
            if (await imgLocator.CountAsync() > 0)
            {
                var alt = await imgLocator.GetAttributeAsync(
                    "alt",
                    new LocatorGetAttributeOptions { Timeout = 1000 });
                AddTokensFromTitle(tags, alt);
            }
        }
        catch
        {
            // Same: best effort.
        }

        // Strategy 3: fall back to title tokens if we still have nothing useful.
        if (tags.Count == 0)
            AddTokensFromTitle(tags, title);

        return tags
            .Where(t => t.Length >= 3)
            .Take(20)
            .ToArray();
    }

    private static void AddCleanedTag(HashSet<string> bag, string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return;
        var cleaned = raw.Trim().Trim(',', ';', '.').ToLowerInvariant();
        if (cleaned.Length is >= 2 and <= 64)
            bag.Add(cleaned);
    }

    private static void AddTokensFromTitle(HashSet<string> bag, string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return;

        var lowered = title.ToLowerInvariant();
        foreach (var noise in new[] { "stock video", "stock footage", "royalty free", "4k", "hd " })
            lowered = lowered.Replace(noise, " ", StringComparison.Ordinal);

        var tokens = lowered.Split(
            new[] { ' ', ',', ';', '.', '-', '|', '/', '\t', '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens)
        {
            if (token.Length < 3)
                continue;
            if (StopWords.Contains(token))
                continue;
            bag.Add(token);
        }
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "the", "with", "from", "into", "onto", "over", "under",
        "for", "this", "that", "these", "those", "video", "clip", "footage"
    };

    private Task HumanDelayAsync(CancellationToken cancellationToken)
    {
        var delay = _random.Next(_options.MinDelayMs, _options.MaxDelayMs + 1);
        return Task.Delay(delay, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_playwright is not null)
        {
            _playwright.Dispose();
            _playwright = null;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Installs popup / extra-page closers on the active context and page
    /// for the SCRAPING phase only. Must be called AFTER authentication has
    /// finished — installing these during CreateSessionAsync would kill the
    /// legitimate Google OAuth popup the user uses to sign in.
    ///
    /// <para>
    /// During scraping any new page is suspicious:
    /// related-clip links inside the hover overlay, signup nags, the rare
    /// download-in-new-tab CTA. Closing them keeps our search-results page
    /// authoritative and prevents the TargetClosedException class of bugs.
    /// </para>
    ///
    /// <para>
    /// Two complementary handlers:
    /// <list type="bullet">
    ///   <item><c>page.Popup</c> — fires for tabs opened via window.open
    ///   from scripts running on <c>mainPage</c>. Closing the popup keeps
    ///   focus on the search results.</item>
    ///   <item><c>context.Page</c> — fires for any further pages created in
    ///   the context. We compare by reference to ensure we never accidentally
    ///   close OUR main page; that case is filtered out explicitly.</item>
    /// </list>
    /// </para>
    /// </summary>
    private void InstallScrapingPopupGuards(IBrowserContext context, IPage mainPage)
    {
        mainPage.Popup += async (_, popup) =>
        {
            _logger.LogWarning(
                "Closed unexpected popup ({Url}) to preserve session continuity.",
                popup.Url);
            try { await popup.CloseAsync(); }
            catch { /* best-effort */ }
        };
        context.Page += async (_, newPage) =>
        {
            if (ReferenceEquals(newPage, mainPage))
                return; // never close the page we're actually using
            _logger.LogWarning(
                "Closed unexpected context page ({Url}) to preserve session continuity.",
                newPage.Url);
            try { await newPage.CloseAsync(); }
            catch { /* best-effort */ }
        };
    }

    private sealed record BrowserSession(IBrowser Browser, IBrowserContext Context, IPage Page) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await Context.CloseAsync();
            await Browser.CloseAsync();
        }
    }
}
