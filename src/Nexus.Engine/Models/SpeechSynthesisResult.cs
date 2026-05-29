namespace Nexus.Engine.Models;

/// <summary>
/// Output of <see cref="Interfaces.ITextToSpeechProvider.SynthesizeAsync"/>.
/// </summary>
/// <param name="FilePath">
/// Absolute path to the audio file written to disk.
/// </param>
/// <param name="MediaType">
/// MIME type of the rendered audio (e.g. <c>audio/mpeg</c> for MP3,
/// <c>audio/wav</c> for WAV). Lets downstream consumers (FFmpeg muxer,
/// HTTP responses) set headers/codecs without sniffing the file.
/// </param>
/// <param name="Duration">
/// Best-effort spoken duration. May be <c>null</c> when the underlying
/// provider doesn't report it; consumers that need an exact duration
/// should probe the file with FFprobe.
/// </param>
/// <param name="HumanizedText">
/// The exact string sent to the TTS API after the humaniser pre-pass.
/// Surfaced for logging/debugging and so subtitle generators downstream
/// can align timestamps against the same text the voice actually spoke.
/// </param>
public sealed record SpeechSynthesisResult(
    string FilePath,
    string MediaType,
    TimeSpan? Duration,
    string HumanizedText);
