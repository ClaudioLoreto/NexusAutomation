namespace Nexus.Core.Configuration;

public class ScraperOptions
{
    public const string SectionName = "Scraper";
    public string CookieFilePath { get; set; } = "cookies.json";
    public int MinDelayMs { get; set; } = 3000;
    public int MaxDelayMs { get; set; } = 7000;
    public string DownloadDirectory { get; set; } = "Downloads";
    public bool HeadlessBrowser { get; set; } = true;

    public StoryblocksSelectors Selectors { get; set; } = new();
}

public class StoryblocksSelectors
{
    public string EmailInput { get; set; } = string.Empty;
    public string PasswordInput { get; set; } = string.Empty;
    public string LoginButton { get; set; } = string.Empty;
    public string SearchBar { get; set; } = string.Empty;
    public string DownloadButton { get; set; } = string.Empty;
}
