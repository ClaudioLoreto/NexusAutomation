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
        var page = session.Page!;

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
        for (var i = 0; i < toDownload; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var card = cards.Nth(i);

            try
            {
                var savedPath = await DownloadFromCardAsync(page, card, downloadDir, cancellationToken);
                var title = await card.GetAttributeAsync("aria-label")
                    ?? await card.Locator("img").First.GetAttributeAsync("alt");

                results.Add(new ScrapedMediaResult(
                    savedPath,
                    title,
                    Tags: Array.Empty<string>()));

                _logger.LogInformation("Downloaded clip {Index}/{Total}: {Path}", i + 1, toDownload, savedPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download card index {Index}", i);
            }

            await HumanDelayAsync(cancellationToken);
        }

        return results;
    }

    private async Task<BrowserSession> CreateSessionAsync(
        bool requireSearchReady,
        CancellationToken cancellationToken)
    {
        _playwright ??= await Playwright.CreateAsync();
        var cookiePath = Path.GetFullPath(_options.CookiePath);
        var hasCookies = CookiePersistence.Exists(cookiePath);

        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = hasCookies,
            Args = ["--disable-blink-features=AutomationControlled"]
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            AcceptDownloads = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            Locale = "en-US",
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        });

        if (hasCookies)
        {
            _logger.LogInformation("Loading cookies from {CookiePath}", cookiePath);
            await CookiePersistence.LoadAsync(context, cookiePath, cancellationToken);
        }

        var page = await context.NewPageAsync();
        await page.GotoAsync(_options.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 60_000
        });

        if (!hasCookies)
        {
            await WaitForManualGoogleLoginAsync(page, cookiePath, cancellationToken);
        }

        if (requireSearchReady)
            await page.WaitForSelectorAsync(StoryblocksSelectors.SearchInput, new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 60_000
            });

        return new BrowserSession(browser, context, page);
    }

    private async Task WaitForManualGoogleLoginAsync(
        IPage page,
        string cookiePath,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "No cookies.json found. Browser opened in HEADED mode — sign in with Google manually.");
        Console.WriteLine();
        Console.WriteLine("=== Storyblocks manual login ===");
        Console.WriteLine("1. Click \"Sign in with Google\" in the browser window.");
        Console.WriteLine("2. Complete OAuth in the browser.");
        Console.WriteLine("3. Wait until the Storyblocks search bar is visible.");
        Console.WriteLine($"   (timeout: {_options.ManualLoginTimeoutMinutes} minutes)");
        Console.WriteLine();

        var timeoutMs = _options.ManualLoginTimeoutMinutes * 60_000;
        await page.WaitForSelectorAsync(StoryblocksSelectors.SearchInput, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });

        await CookiePersistence.SaveAsync(page.Context, cookiePath, cancellationToken);
        _logger.LogInformation("Session cookies saved to {CookiePath}", cookiePath);
        Console.WriteLine($"Cookies saved to: {cookiePath}");
    }

    private async Task RunSearchWithFiltersAsync(IPage page, string query, CancellationToken cancellationToken)
    {
        var search = page.Locator(StoryblocksSelectors.SearchInput).First;
        await search.ClickAsync();
        await HumanDelayAsync(cancellationToken);
        await search.FillAsync(query);
        await HumanDelayAsync(cancellationToken);

        var submit = page.Locator(StoryblocksSelectors.SearchSubmit).First;
        if (await submit.IsVisibleAsync())
            await submit.ClickAsync();
        else
            await search.PressAsync("Enter");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 90_000 });
        await HumanDelayAsync(cancellationToken);

        var filtersBtn = page.GetByRole(AriaRole.Button, new() { Name = "Filters" });
        await filtersBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 60_000 });
        await filtersBtn.ClickAsync();
        await HumanDelayAsync(cancellationToken);

        await page.Locator(StoryblocksSelectors.FilterFootage).ClickAsync();
        await HumanDelayAsync(cancellationToken);
        await page.Locator(StoryblocksSelectors.FilterVertical).ClickAsync();
        await HumanDelayAsync(cancellationToken);
        await page.Locator(StoryblocksSelectors.Filter4K).ClickAsync();
        await HumanDelayAsync(cancellationToken);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 90_000 });
        await page.Locator(StoryblocksSelectors.VideoCard).First.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 60_000 });
    }

    private async Task<string> DownloadFromCardAsync(
        IPage page,
        ILocator card,
        string downloadDir,
        CancellationToken cancellationToken)
    {
        await card.ScrollIntoViewIfNeededAsync();
        await card.HoverAsync(new LocatorHoverOptions { Force = true });
        await HumanDelayAsync(cancellationToken);

        var downloadButton = await ResolveDownloadButtonAsync(page, card);
        await downloadButton.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000
        });

        var downloadTask = page.WaitForDownloadAsync(new PageWaitForDownloadOptions
        {
            Timeout = _options.DownloadTimeoutSeconds * 1000
        });

        await downloadButton.ClickAsync();
        var download = await downloadTask;

        var suggested = download.SuggestedFilename;
        if (string.IsNullOrWhiteSpace(suggested) || !suggested.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            suggested = $"storyblocks_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.mp4";

        var targetPath = Path.Combine(downloadDir, suggested);
        await download.SaveAsAsync(targetPath);

        if (!File.Exists(targetPath))
            throw new InvalidOperationException($"Download did not persist to disk: {targetPath}");

        return targetPath;
    }

    private static async Task<ILocator> ResolveDownloadButtonAsync(IPage page, ILocator card)
    {
        var scoped4K = card.Locator(StoryblocksSelectors.Download4K);
        if (await scoped4K.CountAsync() > 0 && await scoped4K.First.IsVisibleAsync())
            return scoped4K.First;

        var scopedHd = card.Locator(StoryblocksSelectors.DownloadHd);
        if (await scopedHd.CountAsync() > 0 && await scopedHd.First.IsVisibleAsync())
            return scopedHd.First;

        var page4K = page.Locator(StoryblocksSelectors.Download4K);
        if (await page4K.CountAsync() > 0)
            return page4K.First;

        return page.Locator(StoryblocksSelectors.DownloadHd).First;
    }

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

    private sealed record BrowserSession(IBrowser Browser, IBrowserContext Context, IPage Page) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await Context.CloseAsync();
            await Browser.CloseAsync();
        }
    }
}
