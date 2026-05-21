namespace Nexus.Core.DTOs;

public record TtsResult
{
    public bool Success { get; init; }
    public string AudioFilePath { get; init; } = string.Empty;
    public double DurationSeconds { get; init; }
    public string? ErrorMessage { get; init; }
}
