using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public record RenderRequest
{
    public Guid VideoId { get; init; }
    public string MediaFilePath { get; init; } = string.Empty;
    public string AudioFilePath { get; init; } = string.Empty;
    public string ScriptText { get; init; } = string.Empty;
    public NicheType Niche { get; init; }
    public MusicGenre MusicGenre { get; init; }
    public string MusicDirectoryPath { get; init; } = string.Empty;
}
