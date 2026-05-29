namespace Nexus.Engine.Configuration;

/// <summary>
/// Per-niche karaoke subtitle styling. Drives the ASS file written by
/// <c>AssKaraokeWriter</c> and the <c>subtitles=</c> filter in the
/// FFmpeg burn-in pass.
///
/// <para>
/// Legacy YouTubeAutomation hard-coded these values inside
/// <c>SubtitleService</c>. We surface them as a record so each
/// <c>Niche</c> can override them — that's how a dramatic-history short
/// gets gold karaoke and a brainrot short gets neon yellow without
/// touching code.
/// </para>
///
/// <para>
/// Color fields are <c>#RRGGBB</c> hex strings (NOT ASS-encoded). The
/// writer converts them to ASS BGR at render time so config files stay
/// human-readable.
/// </para>
/// </summary>
public sealed record KaraokeStyle
{
    /// <summary>
    /// Font family installed on the host machine. Matches the <c>fontname</c>
    /// field in the ASS Style line.
    /// </summary>
    public string FontFamily { get; init; } = "The Bold Font";

    /// <summary>
    /// Base size for non-highlighted words. Legacy default: 96.
    /// </summary>
    public int FontSize { get; init; } = 96;

    /// <summary>
    /// Pop-out size for the currently-spoken word. Legacy default: 140
    /// (≈ 45% larger). The size animation runs through ASS <c>\t()</c>
    /// alongside the colour change — same animation tick.
    /// </summary>
    public int HighlightFontSize { get; init; } = 140;

    /// <summary>Hex <c>#RRGGBB</c> for inactive words.</summary>
    public string FillColor { get; init; } = "#FFFFFF";

    /// <summary>Hex <c>#RRGGBB</c> for the currently-spoken word.</summary>
    public string HighlightColor { get; init; } = "#FFFF00";

    /// <summary>Hex <c>#RRGGBB</c> for the text outline.</summary>
    public string OutlineColor { get; init; } = "#000000";

    /// <summary>Hex <c>#RRGGBB</c> for the soft text background block.</summary>
    public string BackgroundColor { get; init; } = "#0D1321";

    /// <summary>
    /// Vertical position as a percentage of the canvas height (0 = top,
    /// 100 = bottom). Maps to ASS Style <c>MarginV</c> internally.
    /// Legacy default: bottom-aligned with ~7% margin from the bottom.
    /// </summary>
    public double YPositionPercent { get; init; } = 7.0;

    /// <summary>
    /// Maximum number of words per subtitle line before we wrap to the
    /// next event. Lower = denser cuts, higher = longer flowing lines.
    /// </summary>
    public int MaxWordsPerLine { get; init; } = 5;

    /// <summary>
    /// Minimum pause (seconds) between two consecutive words that forces
    /// a new subtitle line — the legacy heuristic for natural breath
    /// boundaries.
    /// </summary>
    public double LineBreakPauseSeconds { get; init; } = 0.3;
}
