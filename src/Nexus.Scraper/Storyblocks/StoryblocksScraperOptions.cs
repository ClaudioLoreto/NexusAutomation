namespace Nexus.Scraper.Storyblocks;

public sealed class StoryblocksScraperOptions
{
    public const string SectionName = "Storyblocks";

    public string BaseUrl { get; set; } = StoryblocksSelectors.BaseUrl;
    // Path to the Playwright StorageState file (cookies + localStorage +
    // sessionStorage). Renamed from CookiePath because the previous
    // cookie-only persistence broke on Storyblocks — the React app stores
    // its JWT in localStorage, which only StorageStateAsync captures.
    public string SessionStatePath { get; set; } = "./data/state.json";
    public string DownloadDirectory { get; set; } = "./data/downloads";
    public int MinDelayMs { get; set; } = 3000;
    public int MaxDelayMs { get; set; } = 7000;
    public int ManualLoginTimeoutMinutes { get; set; } = 15;
    public int DownloadTimeoutSeconds { get; set; } = 300;
}
