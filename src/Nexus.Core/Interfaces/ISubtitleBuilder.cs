namespace Nexus.Core.Interfaces;

public interface ISubtitleBuilder
{
    /// <summary>
    /// Produces an Advanced SubStation Alpha (.ass) file that displays the
    /// script word-by-word, centered, in a massive bold font, with the
    /// currently spoken word highlighted in <c>#FFD700</c>.
    /// </summary>
    Task<string> BuildAssFileAsync(
        IReadOnlyList<WordTiming> wordTimings,
        string outputFilePath,
        CancellationToken ct = default);
}
