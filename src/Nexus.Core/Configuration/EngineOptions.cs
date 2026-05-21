namespace Nexus.Core.Configuration;

public class EngineOptions
{
    public const string SectionName = "Engine";
    public string FfmpegPath { get; set; } = "ffmpeg";
    public string OutputDirectory { get; set; } = "Output";
    public string LutDirectory { get; set; } = "Assets/Luts";
    public string MusicBaseDirectory { get; set; } = "Assets/Music";
    public int DuckingLevelDb { get; set; } = -22;
    public double MicroZoomMinPercent { get; set; } = 1.0;
    public double MicroZoomMaxPercent { get; set; } = 2.0;
    public string SubtitleFontName { get; set; } = "Arial Bold";
    public int SubtitleFontSize { get; set; } = 48;
    public string SubtitleHighlightColor { get; set; } = "&H0000D7FF";
}
