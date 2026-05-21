using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public sealed record VoiceSynthesisResult(
    string AudioFilePath,
    TimeSpan Duration,
    IReadOnlyList<WordTiming> WordTimings);

public sealed record WordTiming(string Word, TimeSpan Start, TimeSpan End);

public interface IVoiceSynthesizer
{
    Task<VoiceSynthesisResult> SynthesizeAsync(
        string ssml,
        NicheKey niche,
        string outputFilePath,
        CancellationToken ct = default);
}
