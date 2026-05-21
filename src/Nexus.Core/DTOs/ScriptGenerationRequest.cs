using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public record ScriptGenerationRequest
{
    public Guid VideoId { get; init; }
    public NicheType Niche { get; init; }
    public ScriptTone Tone { get; init; }
    public string[] MediaTags { get; init; } = [];
    public string? FirstFrameBase64 { get; init; }
    public int TargetDurationSeconds { get; init; } = 55;
}
