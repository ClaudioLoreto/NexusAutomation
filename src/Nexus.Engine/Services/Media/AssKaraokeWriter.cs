using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Nexus.Engine.Configuration;
using Nexus.Engine.Models;

namespace Nexus.Engine.Services.Media;

/// <summary>
/// Writes a 1080×1920 vertical karaoke subtitle file in the Advanced
/// SubStation Alpha (ASS) format, with one event per logical line and
/// inline <c>\t()</c> animation tags that flip the colour and font size
/// of the word currently being spoken.
///
/// <para>
/// This is a faithful port of the legacy YouTubeAutomation V.4
/// <c>SubtitleService.GenerateAssKaraokeSubtitles</c> (which deliberately
/// avoids classic <c>{\k}</c> karaoke tags because libass renders <c>\t()</c>
/// transitions far smoother on top of vertical Storyblocks footage).
/// Per-niche styling (font, sizes, colours, line-break thresholds, vertical
/// position) is now first-class via <see cref="KaraokeStyle"/> instead of
/// being hardcoded.
/// </para>
/// </summary>
public sealed class AssKaraokeWriter
{
    private const int CanvasWidth = 1080;
    private const int CanvasHeight = 1920;
    private const int ResetDelayMs = 90; // reset window after a word ends, in ms

    private readonly ILogger<AssKaraokeWriter> _logger;

    public AssKaraokeWriter(ILogger<AssKaraokeWriter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates the ASS file at <paramref name="outputPath"/> from the
    /// supplied <paramref name="words"/> using <paramref name="style"/>.
    /// Returns the full path written.
    /// </summary>
    public async Task<string> WriteAsync(
        IReadOnlyList<WordTiming> words,
        KaraokeStyle style,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(words);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (words.Count == 0)
            throw new InvalidOperationException(
                "Cannot write a karaoke file from an empty word-timing list.");

        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var lines = SplitIntoLines(words, style);
        var ass = BuildAssDocument(lines, style);

        await File.WriteAllTextAsync(outputPath, ass, Encoding.UTF8, cancellationToken);
        _logger.LogInformation(
            "Wrote {EventCount} ASS karaoke event(s) for {WordCount} word(s) → {OutputPath}",
            lines.Count, words.Count, outputPath);
        return outputPath;
    }

    /// <summary>
    /// Groups words into subtitle events. A new event begins when:
    ///   1. The pause before the next word exceeds <see cref="KaraokeStyle.LineBreakPauseSeconds"/>, OR
    ///   2. The current event already has <see cref="KaraokeStyle.MaxWordsPerLine"/> words.
    /// </summary>
    private static List<List<WordTiming>> SplitIntoLines(
        IReadOnlyList<WordTiming> words,
        KaraokeStyle style)
    {
        var lines = new List<List<WordTiming>>();
        var current = new List<WordTiming>();

        for (var i = 0; i < words.Count; i++)
        {
            current.Add(words[i]);

            var atMax = current.Count >= style.MaxWordsPerLine;
            var nextHasLongPause = i + 1 < words.Count
                && (words[i + 1].StartSeconds - words[i].EndSeconds) >= style.LineBreakPauseSeconds;
            var isLast = i == words.Count - 1;

            if (atMax || nextHasLongPause || isLast)
            {
                lines.Add(current);
                current = new List<WordTiming>();
            }
        }

        return lines;
    }

    private static string BuildAssDocument(IReadOnlyList<List<WordTiming>> lines, KaraokeStyle style)
    {
        var fillAss = HexToAss(style.FillColor);
        var highlightAss = HexToAss(style.HighlightColor);
        var outlineAss = HexToAss(style.OutlineColor);
        var backgroundAss = HexToAss(style.BackgroundColor);

        // ASS MarginV is measured from the BOTTOM. We translate "Y as a
        // percent of canvas height" (0 = top) into the equivalent
        // bottom-margin pixels so a Y of 95% lands ~5% from the bottom.
        var marginVPx = (int)Math.Round(CanvasHeight * (1.0 - style.YPositionPercent / 100.0));
        if (marginVPx < 0) marginVPx = 0;

        var sb = new StringBuilder();
        sb.AppendLine("[Script Info]");
        sb.AppendLine("ScriptType: v4.00+");
        sb.AppendLine("Collisions: Normal");
        sb.AppendLine($"PlayResX: {CanvasWidth}");
        sb.AppendLine($"PlayResY: {CanvasHeight}");
        sb.AppendLine("WrapStyle: 2");
        sb.AppendLine();
        sb.AppendLine("[V4+ Styles]");
        sb.AppendLine("Format: Name,Fontname,Fontsize,PrimaryColour,SecondaryColour,OutlineColour,BackColour,Bold,Italic,Underline,StrikeOut,ScaleX,ScaleY,Spacing,Angle,BorderStyle,Outline,Shadow,Alignment,MarginL,MarginR,MarginV,Encoding");
        sb.Append("Style: Default,").Append(style.FontFamily).Append(',')
          .Append(style.FontSize).Append(',')
          .Append(fillAss).Append(',').Append(fillAss).Append(',')
          .Append(outlineAss).Append(',').Append(backgroundAss).Append(',')
          .Append("-1,0,0,0,105,102,0,0,1,7,2,2,80,80,").Append(marginVPx).Append(",1");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("[Events]");
        sb.AppendLine("Format: Layer,Start,End,Style,Name,MarginL,MarginR,MarginV,Effect,Text");

        foreach (var line in lines)
        {
            var lineStart = line[0].StartSeconds;
            var lineEnd = line[^1].EndSeconds;
            sb.Append("Dialogue: 0,")
              .Append(FormatAssTime(lineStart)).Append(',')
              .Append(FormatAssTime(lineEnd)).Append(',')
              .Append("Default,,0,0,0,,")
              .Append(BuildLineText(line, lineStart, style, fillAss, highlightAss, outlineAss, backgroundAss));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds the inline-tag body of one Dialogue line, mirroring the
    /// legacy SubtitleService formula:
    /// <c>{\c&lt;fill&gt;&amp;\3c&lt;outline&gt;&amp;\4c&lt;bg&gt;&amp;\4a&amp;H00&amp;\b1\fs&lt;base&gt;\t(start,end,\c&lt;hi&gt;&amp;\b1\fs&lt;hi&gt;)\t(end,reset,\c&lt;fill&gt;&amp;\b1\fs&lt;base&gt;)}word </c>
    /// (one such block per word, joined by a single space).
    /// </summary>
    private static string BuildLineText(
        IReadOnlyList<WordTiming> line,
        double lineStartSeconds,
        KaraokeStyle style,
        string fillAss,
        string highlightAss,
        string outlineAss,
        string backgroundAss)
    {
        var text = new StringBuilder();
        foreach (var word in line)
        {
            var startMs = (int)Math.Round((word.StartSeconds - lineStartSeconds) * 1000d, MidpointRounding.AwayFromZero);
            var endMs = (int)Math.Round((word.EndSeconds - lineStartSeconds) * 1000d, MidpointRounding.AwayFromZero);
            if (startMs < 0) startMs = 0;
            if (endMs <= startMs) endMs = startMs + 1;
            var resetMs = endMs + ResetDelayMs;

            text.Append('{');
            text.Append("\\c").Append(fillAss).Append('&');
            text.Append("\\3c").Append(outlineAss).Append('&');
            text.Append("\\4c").Append(backgroundAss).Append('&');
            text.Append("\\4a&H00&");
            text.Append("\\b1");
            text.Append("\\fs").Append(style.FontSize);
            text.Append("\\t(").Append(startMs).Append(',').Append(endMs).Append(',');
            text.Append("\\c").Append(highlightAss).Append('&');
            text.Append("\\b1\\fs").Append(style.HighlightFontSize).Append(')');
            text.Append("\\t(").Append(endMs).Append(',').Append(resetMs).Append(',');
            text.Append("\\c").Append(fillAss).Append('&');
            text.Append("\\b1\\fs").Append(style.FontSize).Append(')');
            text.Append('}');
            text.Append(word.Word);
            text.Append(' ');
        }
        return text.ToString().TrimEnd();
    }

    /// <summary>
    /// Converts <c>#RRGGBB</c> → ASS <c>&amp;H00BBGGRR</c> (note the
    /// reversed channel order and the alpha byte at the front).
    /// </summary>
    internal static string HexToAss(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return "&H00FFFFFF";
        var trimmed = hex.TrimStart('#');
        if (trimmed.Length != 6)
            return "&H00FFFFFF";
        var r = trimmed.Substring(0, 2);
        var g = trimmed.Substring(2, 2);
        var b = trimmed.Substring(4, 2);
        return $"&H00{b}{g}{r}".ToUpperInvariant();
    }

    /// <summary>Formats seconds as ASS h:mm:ss.cc (centiseconds).</summary>
    internal static string FormatAssTime(double seconds)
    {
        if (seconds < 0) seconds = 0;
        var ts = TimeSpan.FromSeconds(seconds);
        var cs = (int)Math.Round((ts.Milliseconds) / 10.0, MidpointRounding.AwayFromZero);
        if (cs >= 100) cs = 99;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}.{cs:00}");
    }
}
