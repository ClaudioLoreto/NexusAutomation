using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

/// <summary>
/// Niche projection exposed by <c>/api/niches</c>. Mirrors every editorial
/// + render-pipeline knob on the <c>Niche</c> entity so the dashboard can
/// drive a complete "Create / Edit Niche" form without a second round-trip.
/// </summary>
public sealed record NicheDto(
    int Id,
    NicheType Type,
    string Name,
    string LanguageCode,
    string ScriptTone,
    int? TargetWordCount,
    int? MaxWords,
    string TtsVoice,
    float TtsSpeed,
    string ElevenLabsVoiceId,
    string MusicDirectory,
    string KaraokeFontFamily,
    int KaraokeFontSize,
    int KaraokeHighlightFontSize,
    string KaraokeFillColor,
    string KaraokeHighlightColor,
    string KaraokeOutlineColor,
    string KaraokeBackgroundColor,
    double KaraokeYPositionPercent,
    string OverlayGifPath,
    double OverlayGifPositionPercent,
    double OverlayGifTailSeconds,
    int OverlayGifLoopCount,
    string AdditionalScriptInstructions,
    bool IsActive,
    int QueuePriority,
    int VideoCount);
