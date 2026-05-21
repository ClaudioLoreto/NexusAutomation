namespace Nexus.Core.Interfaces;

public interface ISubtitleGenerator
{
    Task<string> GenerateAssSubtitlesAsync(string scriptText, double audioDurationSeconds, string outputPath, CancellationToken ct = default);
}
