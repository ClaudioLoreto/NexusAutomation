namespace Nexus.Engine.Models;

/// <summary>
/// Input to <see cref="Interfaces.ITextToSpeechProvider.SynthesizeAsync"/>.
/// </summary>
/// <param name="Text">
/// The RAW script text. The TTS implementation will run the registered
/// <see cref="Interfaces.ITextHumanizer"/> chain over it before dispatching
/// the request to the underlying provider — callers should NOT pre-humanise.
/// </param>
/// <param name="LanguageCode">
/// BCP-47 language tag, e.g. <c>"it-IT"</c> or <c>"en-US"</c>. Drives both
/// the humaniser rules and any provider-side language hint.
/// </param>
/// <param name="OutputPath">
/// Absolute or relative path where the resulting audio file should be
/// written. The implementation creates parent directories as needed.
/// </param>
/// <param name="Voice">
/// Provider-specific voice identifier. For OpenAI TTS this is one of
/// <c>alloy</c>, <c>echo</c>, <c>fable</c>, <c>onyx</c>, <c>nova</c>,
/// <c>shimmer</c>. Null means "use the provider default from settings".
/// </param>
/// <param name="Speed">
/// Optional playback speed multiplier. <c>1.0</c> is the natural rate.
/// Range and validation are provider-specific.
/// </param>
public sealed record SpeechSynthesisRequest(
    string Text,
    string LanguageCode,
    string OutputPath,
    string? Voice = null,
    float? Speed = null);
