namespace Nexus.Core.DTOs;

public record ScriptGenerationResult
{
    public bool Success { get; init; }
    public string ScriptText { get; init; } = string.Empty;
    public string SsmlText { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
}
