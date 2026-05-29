using Nexus.Engine.Configuration;

namespace Nexus.Engine.Models;

/// <summary>
/// Input to <see cref="Interfaces.IVideoAssembler.AssembleAsync"/>.
///
/// <para>
/// Storyblocks clips are the ONLY source of raw visual media in Nexus —
/// the karaoke subtitle pass and the Subscribe GIF overlay are therefore
/// always applied (mirroring the legacy YouTubeAutomation V.4 pipeline).
/// </para>
/// </summary>
/// <param name="ClipPaths">
/// Ordered list of Storyblocks clip files to concatenate as the visual
/// track. At least one path is required. The legacy pipeline uses 5 clips
/// per 30–60s short.
/// </param>
/// <param name="VoiceoverPath">
/// Path to the synthesised voiceover (typically MP3 from
/// <see cref="Interfaces.ITextToSpeechProvider"/>). Required — also feeds
/// Whisper for word-level karaoke timings when
/// <see cref="WordTimings"/> is null.
/// </param>
/// <param name="OutputPath">
/// Absolute or relative path where the final MP4 should be written.
/// Parent directories are created as needed.
/// </param>
/// <param name="LanguageCode">
/// BCP-47 language tag (e.g. <c>"it-IT"</c>) used as a hint for the
/// word-timing source when timings need to be derived from
/// <see cref="VoiceoverPath"/>. Defaults to <c>"en-US"</c>.
/// </param>
/// <param name="WordTimings">
/// Optional pre-computed per-word timings. When null, the assembler asks
/// the registered <see cref="Interfaces.IWordTimingSource"/> to derive
/// them from the voiceover. Supply this when the caller already has
/// timings (e.g. from OpenAI TTS speech timestamps) to avoid a redundant
/// Whisper call.
/// </param>
/// <param name="KaraokeStyle">
/// Per-niche karaoke styling. When null, the assembler uses the default
/// <see cref="Configuration.KaraokeStyle"/> registered in DI.
/// </param>
/// <param name="OverlayGif">
/// Per-niche Subscribe-GIF overlay configuration. When null, the
/// assembler uses the default <see cref="OverlayGifSettings"/> registered
/// in DI. Set <see cref="OverlayGifSettings.AssetPath"/> to empty to skip
/// the overlay pass entirely.
/// </param>
/// <param name="MusicPath">
/// Optional background music track. Will be mixed under the voiceover
/// (volume + sidechain compressor + loudnorm chain ported from V.4).
/// </param>
/// <param name="MusicVolume">
/// Background music volume multiplier (legacy default <c>0.32</c>).
/// </param>
/// <param name="VoiceVolume">
/// Voice volume multiplier before the mix (legacy default <c>1.0</c>).
/// </param>
/// <param name="MaxDurationSeconds">
/// Hard cap on the final clip length, in seconds. The legacy pipeline
/// trims at <c>58.0</c> for YouTube Shorts compliance. Null disables the
/// cap.
/// </param>
public sealed record VideoAssemblyRequest(
    IReadOnlyList<string> ClipPaths,
    string VoiceoverPath,
    string OutputPath,
    string LanguageCode = "en-US",
    IReadOnlyList<WordTiming>? WordTimings = null,
    KaraokeStyle? KaraokeStyle = null,
    OverlayGifSettings? OverlayGif = null,
    string? MusicPath = null,
    float MusicVolume = 0.32f,
    float VoiceVolume = 1.0f,
    double? MaxDurationSeconds = 58.0);
