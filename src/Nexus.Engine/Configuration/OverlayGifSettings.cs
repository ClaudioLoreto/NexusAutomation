namespace Nexus.Engine.Configuration;

/// <summary>
/// Per-niche "Subscribe" GIF overlay configuration. Drives the third
/// FFmpeg pass in <c>FFmpegVideoAssembler</c>.
/// </summary>
public sealed record OverlayGifSettings
{
    /// <summary>
    /// Absolute or relative path to the GIF/WebM/MOV asset. Empty string
    /// means "no overlay" — the assembler will skip the third pass and
    /// just rename the post-music video to the final output path.
    /// </summary>
    public string AssetPath { get; init; } = string.Empty;

    /// <summary>
    /// Vertical position as a percentage of the canvas height (0 = top,
    /// 100 = bottom). Legacy default: ~95% (90 px above bottom on a
    /// 1920-tall canvas ≈ 95.3%).
    /// </summary>
    public double YPositionPercent { get; init; } = 95.3;

    /// <summary>
    /// How long (seconds) before the end of the final video the overlay
    /// should appear. Legacy default: 5.0 — the GIF plays for the LAST
    /// 5 seconds.
    /// </summary>
    public double TailSeconds { get; init; } = 5.0;

    /// <summary>
    /// Times to loop the GIF once it starts. <c>0</c> means "play once",
    /// <c>-1</c> means "loop until the video ends".
    /// </summary>
    public int LoopCount { get; init; } = 0;
}
