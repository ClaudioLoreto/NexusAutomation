namespace Nexus.Scraper.Storyblocks;

/// <summary>
/// DOM selectors for Storyblocks unified nav UI (2025+).
/// </summary>
internal static class StoryblocksSelectors
{
    public const string BaseUrl = "https://www.storyblocks.com";

    public const string SearchInput =
        "input[aria-label='Input Search: '], input[placeholder='Search video library...']";

    public const string SearchSubmit =
        "button[aria-label='Submit Search']";

    public const string FiltersToggle =
        "button:has(span:text-is('Filters'))";

    public const string FilterFootage = "#MediaTypefootage";
    public const string FilterVertical = "#Orientationvertical";
    public const string Filter4K = "#Resolution4K";

    public const string VideoCard =
        "[data-testid='video-stock-item-card']";

    public const string Download4K =
        "button.download-triggers[title*='4K MP4']";

    public const string DownloadHd =
        "button.download-triggers[title*='HD MP4']";
}
