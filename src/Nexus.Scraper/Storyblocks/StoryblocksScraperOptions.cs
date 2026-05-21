namespace Nexus.Scraper.Storyblocks;

public sealed class StoryblocksScraperOptions
{
    public const string SectionName = "Storyblocks";

    public string BaseUrl { get; set; } = StoryblocksSelectors.BaseUrl;
    public string CookiePath { get; set; } = "./data/cookies.json";
    public string DownloadDirectory { get; set; } = "./data/downloads";
    public int MinDelayMs { get; set; } = 3000;
    public int MaxDelayMs { get; set; } = 7000;
    public int ManualLoginTimeoutMinutes { get; set; } = 15;
    public int DownloadTimeoutSeconds { get; set; } = 300;
}
