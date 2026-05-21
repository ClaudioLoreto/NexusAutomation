namespace Nexus.Core.DTOs;

public record MediaDownloadResult
{
    public bool Success { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public string[] Tags { get; init; } = [];
    public string? FirstFrameBase64 { get; init; }
    public string? ErrorMessage { get; init; }
    public bool RequiresHumanIntervention { get; init; }
}
