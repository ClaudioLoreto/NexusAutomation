using Nexus.Core.Enums;

namespace Nexus.Core.Dtos;

public sealed record RenderRequestDto(
    Guid VideoId,
    NicheKey Niche,
    string VoiceOverFilePath,
    string SubtitleFilePath,
    string BackgroundMusicFilePath,
    IReadOnlyList<StoryblocksClipDto> Clips,
    string OutputFilePath,
    int OutputWidth = 1080,
    int OutputHeight = 1920,
    int OutputFps = 30,
    double MusicDuckDb = -22.0,
    double MicroZoomMin = 1.01,
    double MicroZoomMax = 1.02);
