using Nexus.Engine.Models;

namespace Nexus.Engine.Interfaces;

/// <summary>
/// Synthesises spoken audio from raw script text.
///
/// <para>
/// Implementations MUST run their <see cref="ITextHumanizer"/> pre-pass before
/// dispatching the request to the underlying TTS API — this is how we keep
/// pronunciation of numbers, Roman numerals, and (future) dates/acronyms
/// consistent across providers (OpenAI, ElevenLabs, future Azure/Polly).
/// </para>
///
/// <para>
/// The interface is provider-agnostic by design. Provider-specific knobs
/// (ElevenLabs <c>stability</c>/<c>similarity_boost</c>, OpenAI <c>speed</c>,
/// streaming flags, response format) live on the implementation's settings
/// class, not in the contract — so swapping providers never breaks callers.
/// </para>
/// </summary>
public interface ITextToSpeechProvider
{
    Task<SpeechSynthesisResult> SynthesizeAsync(
        SpeechSynthesisRequest request,
        CancellationToken cancellationToken = default);
}
