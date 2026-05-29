using Nexus.Engine.Models;

namespace Nexus.Engine.Interfaces;

/// <summary>
/// Resolves a list of <see cref="WordTiming"/> entries for an audio file —
/// the data needed to drive word-by-word karaoke.
///
/// <para>
/// The legacy YouTubeAutomation V.4 implementation uses OpenAI Whisper
/// (<c>whisper-1</c> model with <c>timestamp_granularities[]=word</c>);
/// future implementations could call OpenAI's TTS speech-timestamps API
/// or a local <c>whisper.cpp</c> instance. The interface keeps the
/// renderer ignorant of which timing source is in play.
/// </para>
/// </summary>
public interface IWordTimingSource
{
    /// <summary>
    /// Transcribes <paramref name="audioPath"/> into per-word timings,
    /// using <paramref name="languageCode"/> as a hint to the underlying
    /// engine (BCP-47, e.g. <c>"it-IT"</c>, <c>"en-US"</c>).
    /// </summary>
    Task<IReadOnlyList<WordTiming>> GetWordTimingsAsync(
        string audioPath,
        string languageCode,
        CancellationToken cancellationToken = default);
}
