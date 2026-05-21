using Nexus.Core.Enums;

namespace Nexus.Core.Dtos;

public sealed record ScriptRequestDto(
    Guid VideoId,
    NicheKey Niche,
    ScriptTone Tone,
    IReadOnlyList<string> MediaTags,
    IReadOnlyList<string>? FirstFrameImagesBase64,
    int TargetSeconds = 55);
