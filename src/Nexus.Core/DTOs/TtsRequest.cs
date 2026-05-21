using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public record TtsRequest
{
    public Guid VideoId { get; init; }
    public string SsmlText { get; init; } = string.Empty;
    public VoiceStyle VoiceStyle { get; init; }
}
