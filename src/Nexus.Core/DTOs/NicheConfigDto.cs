using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public record NicheConfigDto
{
    public Guid Id { get; init; }
    public NicheType NicheType { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public ScriptTone ScriptTone { get; init; }
    public VoiceStyle VoiceStyle { get; init; }
    public MusicGenre MusicGenre { get; init; }
    public string MusicDirectoryPath { get; init; } = string.Empty;
    public string[] SearchKeywords { get; init; } = [];
    public bool IsActive { get; init; }
    public int Priority { get; init; }
}
