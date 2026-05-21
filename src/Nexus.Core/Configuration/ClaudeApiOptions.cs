namespace Nexus.Core.Configuration;

public class ClaudeApiOptions
{
    public const string SectionName = "ClaudeApi";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-3-haiku-20240307";
    public int MaxTokens { get; set; } = 2048;
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
}
