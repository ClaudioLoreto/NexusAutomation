namespace Nexus.Engine.Models;

/// <summary>
/// One word in a script with its precise spoken time window. Ported from
/// the legacy <c>WordTimestamp</c> model in YouTubeAutomation V.4 (which
/// in turn came from OpenAI Whisper's word-level timestamps).
///
/// <para>Times are in SECONDS, matching Whisper's response shape.</para>
/// </summary>
/// <param name="Word">The exact spoken token (with apostrophes preserved).</param>
/// <param name="StartSeconds">Word onset, seconds from start of the audio file.</param>
/// <param name="EndSeconds">Word offset, seconds from start of the audio file.</param>
public sealed record WordTiming(string Word, double StartSeconds, double EndSeconds);
