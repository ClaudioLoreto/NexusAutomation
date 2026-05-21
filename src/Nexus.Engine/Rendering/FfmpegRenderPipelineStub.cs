namespace Nexus.Engine.Rendering;

/// <summary>
/// Xabe.FFmpeg pipeline: micro-zoom, LUT, ASS subtitles, -22dB ducking — Phase 4+.
/// </summary>
public sealed class FfmpegRenderPipelineStub
{
    public const double DefaultVoiceDuckDb = -22;
    public const double MicroZoomMinPercent = 1.0;
    public const double MicroZoomMaxPercent = 2.0;
}
