namespace Nexus.Scraper.Storyblocks;

/// <summary>
/// DOM selectors for Storyblocks unified nav UI (2025+).
/// </summary>
internal static class StoryblocksSelectors
{
    public const string BaseUrl = "https://www.storyblocks.com";

    // Storyblocks ships TWO search inputs in the DOM at all times: the mobile-nav
    // drawer input (hidden via CSS until the hamburger is opened) and the desktop
    // header input. A naive selector that returns both will make Playwright watch
    // the FIRST DOM match — which is the hidden mobile one — and time out.
    //
    // We solve this in two layers:
    //   1) The selector below uses Playwright's `:visible` engine pseudo-class
    //      so it ONLY resolves to elements with non-empty bounding boxes.
    //   2) Callers in StoryblocksScraper additionally chain
    //      `.Filter(new() { Visible = true }).First` for defense in depth.
    //
    // The clauses are listed best-first and use case-insensitive partial matches
    // so the selector survives copy tweaks and locale changes (e.g. when the
    // placeholder changes from "Search video library..." to "Search videos...").
    public const string SearchInput =
        "input[type='search']:visible, " +
        "input[placeholder*='video library' i]:visible, " +
        "input[aria-label*='Input Search' i]:visible";

    public const string SearchSubmit =
        "button[aria-label*='Submit Search' i]:visible, " +
        "button[type='submit'][aria-label*='search' i]:visible";

    // The Filters drawer toggle. Two clauses:
    //   1) `button:has-text('Filters')` — text-based, English UIs.
    //   2) `button:has(img[alt='filter button'])` — icon-only variant some
    //      Storyblocks experiments render on narrow viewports. The alt attribute
    //      is a code identifier and stays stable across locales.
    // Use Force=false on the click; the parent isn't an anchor here, so a normal
    // click is safe and we want actionability checks (e.g. "is the toggle
    // covered by a cookie banner?") to fire and abort early if something is wrong.
    public const string FiltersToggle =
        "button:has-text('Filters'), " +
        "button:has(img[alt='filter button'])";

    public const string FilterFootage = "#MediaTypefootage";
    public const string FilterVertical = "#Orientationvertical";
    public const string Filter4K = "#Resolution4K";

    public const string VideoCard =
        "[data-testid='video-stock-item-card']";

    // The header "logged-in" indicator — Storyblocks renders this ONLY once
    // the OAuth callback chain has finished and the user is authenticated.
    // We poll on THIS (not the search bar, which exists for logged-out
    // visitors too) before snapshotting session state.
    //
    // CRITICAL: this selector is TAG-AGNOSTIC. Earlier versions anchored on
    // `a[aria-label='Account']`, which timed out on the live site because
    // Storyblocks renders the avatar as a <button>, not an <a>. We now
    // match on STRUCTURAL ATTRIBUTES that survive any tag/layout swap:
    //
    //   1. `[aria-label='Account']` — matches any tag (button, a, div, …)
    //      with the stable code identifier. Survives the button-vs-anchor
    //      flip that broke the previous selector.
    //   2. `img[src*='account.svg']` — fallback for layouts that render
    //      the avatar as a bare SVG icon without the aria-label.
    public const string AccountIndicator =
        "[aria-label='Account'], img[src*='account.svg']";

    // Storyblocks download flow has TWO layers:
    //
    //   1. The card hover overlay paints a generic "Download" CTA on the
    //      bottom-right of each card. Clicking this CTA opens a format-picker
    //      dropdown (rendered by React with `class="download-format-selector"`).
    //      The CTA itself does NOT trigger a download.
    //
    //   2. The format picker contains the real resolution-specific buttons
    //      (HD MP4, 4K MP4, WebM, etc.) which DO trigger downloads. They share
    //      the stable code-identifier class `download-triggers` but their
    //      `title` attribute is per-video and per-codec ("4K MP4 (hevc) - 7.8
    //      MB"), so we never match on title text — we match on the resolution
    //      and codec tokens via Playwright's `Filter(HasText = ...)` API at
    //      the call site.

    // The intermediate hover CTA, mapped from the live DOM via F8 inspection.
    // The element is NOT a <button> — it's a <div role="button">:
    //     <div tabindex="0" class="..." data-cy="download-button"
    //          aria-label="download" role="button">
    //
    // We match on `data-cy="download-button"` first (Storyblocks' own test
    // hook — by definition stable across UI tweaks and locales) and fall
    // back to `aria-label="download"` for resilience against e2e-test cleanup
    // that could one day remove the data-cy attribute. Both clauses match
    // ANY tag (button, div, a) carrying those attributes, since `role`,
    // `data-cy`, and `aria-label` are tag-agnostic.
    //
    // Always queried RELATIVE TO THE CARD (`card.Locator(...)`) so we never
    // accidentally pick up nav/footer download links.
    public const string CardOverlayDownloadButton =
        "[data-cy='download-button'], [aria-label='download']";

    // The resolution-specific buttons inside the format-picker dropdown.
    // Always combined with `.Filter(new() { HasText = "4K" / "HD" })` and
    // `.Filter(new() { HasText = "mp4" })` at the call site, so this constant
    // is intentionally bare — it's just the class anchor, not a full selector.
    public const string BaseDownloadButton = "button.download-triggers";

    // "Related keywords" pills shown on the card hover overlay. Storyblocks renders
    // these as plain <a> elements scoped under the card. If this selector returns
    // nothing the scraper falls back to alt/title text extraction.
    public const string TagChip =
        "a[href*='/video/search?']:not([href*='page='])";
}
