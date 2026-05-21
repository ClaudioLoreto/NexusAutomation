using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Core.Configuration;
using Nexus.Core.Interfaces;

namespace Nexus.Engine.Subtitles;

public class AssSubtitleGenerator : ISubtitleGenerator
{
    private readonly EngineOptions _options;
    private readonly ILogger<AssSubtitleGenerator> _logger;

    public AssSubtitleGenerator(
        IOptions<EngineOptions> options,
        ILogger<AssSubtitleGenerator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateAssSubtitlesAsync(
        string scriptText,
        double audioDurationSeconds,
        string outputPath,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Generating .ass subtitles ({Duration:F1}s)", audioDurationSeconds);

        var words = scriptText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var totalWords = words.Length;

        if (totalWords == 0)
        {
            _logger.LogWarning("Empty script — no subtitles to generate");
            return outputPath;
        }

        var timePerWord = audioDurationSeconds / totalWords;
        var chunkSize = 3;

        var sb = new StringBuilder();
        sb.AppendLine("[Script Info]");
        sb.AppendLine("ScriptType: v4.00+");
        sb.AppendLine("PlayResX: 1080");
        sb.AppendLine("PlayResY: 1920");
        sb.AppendLine("WrapStyle: 0");
        sb.AppendLine();
        sb.AppendLine("[V4+ Styles]");
        sb.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
        sb.AppendLine($"Style: Default,{_options.SubtitleFontName},{_options.SubtitleFontSize},&H00FFFFFF,&H000000FF,&H00000000,&H80000000,-1,0,0,0,100,100,0,0,1,3,1,2,40,40,200,1");
        sb.AppendLine($"Style: Highlight,{_options.SubtitleFontName},{_options.SubtitleFontSize},{_options.SubtitleHighlightColor},&H000000FF,&H00000000,&H80000000,-1,0,0,0,100,100,0,0,1,3,1,2,40,40,200,1");
        sb.AppendLine();
        sb.AppendLine("[Events]");
        sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");

        var currentTime = 0.0;

        for (var i = 0; i < totalWords; i += chunkSize)
        {
            var chunk = words.Skip(i).Take(chunkSize).ToArray();
            var chunkDuration = chunk.Length * timePerWord;
            var startTime = FormatAssTime(currentTime);
            var endTime = FormatAssTime(currentTime + chunkDuration);

            var highlightedChunk = BuildHighlightedChunk(chunk, timePerWord);
            sb.AppendLine($"Dialogue: 0,{startTime},{endTime},Default,,0,0,0,,{highlightedChunk}");

            currentTime += chunkDuration;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        await File.WriteAllTextAsync(outputPath, sb.ToString(), ct);

        _logger.LogInformation("Subtitles written to {Path} ({WordCount} words, {Chunks} chunks)",
            outputPath, totalWords, (totalWords + chunkSize - 1) / chunkSize);

        return outputPath;
    }

    private string BuildHighlightedChunk(string[] words, double timePerWord)
    {
        if (words.Length == 1) return words[0];

        var sb = new StringBuilder();
        for (var i = 0; i < words.Length; i++)
        {
            if (i == 0)
            {
                sb.Append($"{{\\c{_options.SubtitleHighlightColor}}}{words[i]}{{\\c&H00FFFFFF&}}");
            }
            else
            {
                sb.Append($" {words[i]}");
            }
        }
        return sb.ToString();
    }

    private static string FormatAssTime(double totalSeconds)
    {
        var hours = (int)(totalSeconds / 3600);
        var minutes = (int)((totalSeconds % 3600) / 60);
        var seconds = (int)(totalSeconds % 60);
        var centiseconds = (int)((totalSeconds - Math.Floor(totalSeconds)) * 100);
        return $"{hours}:{minutes:D2}:{seconds:D2}.{centiseconds:D2}";
    }
}
