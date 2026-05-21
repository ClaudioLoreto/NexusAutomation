namespace Nexus.Core.DTOs;

public record RenderResult
{
    public bool Success { get; init; }
    public string OutputFilePath { get; init; } = string.Empty;
    public double DurationSeconds { get; init; }
    public string? ErrorMessage { get; init; }
}
