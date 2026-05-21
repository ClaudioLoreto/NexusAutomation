namespace Nexus.Core.Configuration;

public class YouTubeApiOptions
{
    public const string SectionName = "YouTubeApi";
    public string ApiKey { get; set; } = string.Empty;
    public int MaxResultsPerQuery { get; set; } = 50;
    public int ViewVelocityWindowHours { get; set; } = 24;
}
