using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public record VideoDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public NicheType Niche { get; init; }
    public VideoStatus Status { get; init; }
    public string? ScriptText { get; init; }
    public string? MediaFilePath { get; init; }
    public string? AudioFilePath { get; init; }
    public string? OutputFilePath { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}
