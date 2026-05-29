using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Engine.Configuration;
using Nexus.Engine.Interfaces;
using Nexus.Engine.Models;

namespace Nexus.Engine.Services.Media;

/// <summary>
/// Default <see cref="IVideoAssembler"/> implementation. Faithfully ports
/// the legacy YouTubeAutomation V.4 three-pass FFmpeg pipeline:
/// <list type="number">
///   <item><b>Pass 1 – Subtitles burn-in</b>: concat demuxer over the
///   Storyblocks clips, <c>crop=ih*9/16:ih,scale=1080:1920,setsar=1</c>
///   for vertical canvas, <c>subtitles=...</c> (libass) reading the ASS
///   file produced by <see cref="AssKaraokeWriter"/>.</item>
///   <item><b>Pass 2 – Music mix</b>: voice + background music ducked via
///   <c>amix</c> + <c>acompressor</c> + <c>loudnorm</c> (skipped when no
///   music supplied).</item>
///   <item><b>Pass 3 – Subscribe overlay</b>: animated GIF overlaid in
///   the last <c>TailSeconds</c> at the configured Y-position. Skipped
///   when the GIF asset path is empty or missing on disk — the post-music
///   video is renamed to the final output instead.</item>
/// </list>
///
/// Storyblocks-only: subtitles and Subscribe GIF are ALWAYS overlaid on
/// Storyblocks clips. The assembler bakes that assumption in (no
/// per-clip provider switching, no different filtergraphs based on source).
/// </summary>
public sealed class FFmpegVideoAssembler : IVideoAssembler
{
    private const int CanvasWidth = 1080;
    private const int CanvasHeight = 1920;

    private readonly FfmpegRunner _ffmpeg;
    private readonly AssKaraokeWriter _assWriter;
    private readonly IWordTimingSource _wordTimingSource;
    private readonly KaraokeStyle _defaultKaraokeStyle;
    private readonly OverlayGifSettings _defaultOverlay;
    private readonly ILogger<FFmpegVideoAssembler> _logger;

    public FFmpegVideoAssembler(
        FfmpegRunner ffmpeg,
        AssKaraokeWriter assWriter,
        IWordTimingSource wordTimingSource,
        IOptions<KaraokeStyle> defaultKaraokeStyle,
        IOptions<OverlayGifSettings> defaultOverlay,
        ILogger<FFmpegVideoAssembler> logger)
    {
        _ffmpeg = ffmpeg;
        _assWriter = assWriter;
        _wordTimingSource = wordTimingSource;
        _defaultKaraokeStyle = defaultKaraokeStyle.Value;
        _defaultOverlay = defaultOverlay.Value;
        _logger = logger;
    }

    public async Task<VideoAssemblyResult> AssembleAsync(
        VideoAssemblyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ClipPaths.Count == 0)
            throw new ArgumentException("At least one Storyblocks clip is required.", nameof(request));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.VoiceoverPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputPath);
        if (!File.Exists(request.VoiceoverPath))
            throw new FileNotFoundException("Voiceover file not found.", request.VoiceoverPath);

        var outputDir = Path.GetDirectoryName(Path.GetFullPath(request.OutputPath));
        if (!string.IsNullOrEmpty(outputDir))
            Directory.CreateDirectory(outputDir);

        var workDir = Path.Combine(Path.GetTempPath(), "nexus-engine", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);
        _logger.LogInformation("FFmpeg work dir: {WorkDir}", workDir);

        try
        {
            // 1. Resolve word timings (caller-supplied or Whisper).
            var timings = request.WordTimings;
            if (timings is null || timings.Count == 0)
            {
                _logger.LogInformation("No pre-computed word timings; running Whisper on voiceover.");
                timings = await _wordTimingSource.GetWordTimingsAsync(
                    request.VoiceoverPath, request.LanguageCode, cancellationToken);
            }
            if (timings.Count == 0)
                throw new InvalidOperationException(
                    "Could not derive any word timings from the voiceover. Cannot generate karaoke subtitles.");

            // 2. Write the ASS karaoke file.
            var karaokeStyle = request.KaraokeStyle ?? _defaultKaraokeStyle;
            var assPath = Path.Combine(workDir, "karaoke.ass");
            await _assWriter.WriteAsync(timings, karaokeStyle, assPath, cancellationToken);

            // 3. Pass 1 — concat clips + crop/scale to vertical + burn-in subtitles.
            var concatListPath = await WriteConcatListAsync(request.ClipPaths, workDir, cancellationToken);
            var pass1Output = Path.Combine(workDir, "pass1_subs.mp4");
            await RunPass1ConcatAndSubtitlesAsync(
                concatListPath, request.VoiceoverPath, assPath, pass1Output, cancellationToken);

            // 4. Pass 2 — mix background music (or pass-through when no music).
            var pass2Output = await RunPass2MusicMixAsync(
                pass1Output, request, workDir, cancellationToken);

            // 5. Pass 3 — Subscribe GIF overlay (or rename when overlay disabled / missing).
            var overlay = request.OverlayGif ?? _defaultOverlay;
            var finalPath = await RunPass3OverlayAsync(
                pass2Output, overlay, request.OutputPath, cancellationToken);

            var durationSeconds = await _ffmpeg.GetDurationSecondsAsync(finalPath, cancellationToken);
            return new VideoAssemblyResult(
                OutputPath: Path.GetFullPath(finalPath),
                Duration: TimeSpan.FromSeconds(durationSeconds));
        }
        finally
        {
            try
            {
                Directory.Delete(workDir, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not clean up work dir {WorkDir}", workDir);
            }
        }
    }

    // ------------------------------------------------------------------
    //  Pass 1 — concat demuxer + subtitle burn-in
    // ------------------------------------------------------------------

    private static async Task<string> WriteConcatListAsync(
        IReadOnlyList<string> clipPaths,
        string workDir,
        CancellationToken cancellationToken)
    {
        var listPath = Path.Combine(workDir, "concat.txt");
        var lines = new List<string>(clipPaths.Count);
        foreach (var clip in clipPaths)
        {
            if (!File.Exists(clip))
                throw new FileNotFoundException("Storyblocks clip not found.", clip);
            // FFmpeg concat demuxer: paths must use forward slashes and any
            // single quote inside the path needs escaping.
            var ffmpegPath = Path.GetFullPath(clip).Replace('\\', '/').Replace("'", @"\'");
            lines.Add($"file '{ffmpegPath}'");
        }
        await File.WriteAllLinesAsync(listPath, lines, Encoding.UTF8, cancellationToken);
        return listPath;
    }

    private Task RunPass1ConcatAndSubtitlesAsync(
        string concatListPath,
        string voiceoverPath,
        string assPath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var assArg = EscapeForSubtitlesFilter(assPath);
        var filter =
            $"[0:v]crop=ih*9/16:ih,scale={CanvasWidth}:{CanvasHeight},setsar=1,subtitles='{assArg}'[v]";

        var args = string.Format(
            CultureInfo.InvariantCulture,
            "-y -hide_banner -loglevel error " +
            "-f concat -safe 0 -i \"{0}\" -i \"{1}\" " +
            "-filter_complex \"{2}\" " +
            "-map \"[v]\" -map 1:a " +
            "-c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a aac " +
            "-shortest " +
            "\"{3}\"",
            concatListPath, voiceoverPath, filter, outputPath);

        _logger.LogInformation("Pass 1: concat + subtitles → {Output}", outputPath);
        return _ffmpeg.RunFfmpegAsync(args, cancellationToken);
    }

    /// <summary>
    /// Escapes an ASS path so it survives both the shell and the
    /// libavfilter <c>subtitles=</c> argument parser:
    /// backslashes → forward slashes, then <c>:</c> → <c>\:</c> so drive
    /// letters like <c>C:</c> aren't misread as filter delimiters.
    /// </summary>
    private static string EscapeForSubtitlesFilter(string assPath)
    {
        var full = Path.GetFullPath(assPath);
        return full.Replace('\\', '/').Replace(":", "\\:");
    }

    // ------------------------------------------------------------------
    //  Pass 2 — music mix (amix + acompressor + loudnorm)
    // ------------------------------------------------------------------

    private async Task<string> RunPass2MusicMixAsync(
        string pass1Output,
        VideoAssemblyRequest request,
        string workDir,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MusicPath))
        {
            _logger.LogInformation("Pass 2: no music supplied, skipping mix.");
            return pass1Output;
        }
        if (!File.Exists(request.MusicPath))
        {
            _logger.LogWarning(
                "Pass 2: music file {Music} not found, skipping mix.", request.MusicPath);
            return pass1Output;
        }

        var pass2Output = Path.Combine(workDir, "pass2_music.mp4");
        var voiceVol = request.VoiceVolume.ToString("0.00", CultureInfo.InvariantCulture);
        var musicVol = request.MusicVolume.ToString("0.00", CultureInfo.InvariantCulture);
        var targetDuration = await _ffmpeg.GetDurationSecondsAsync(pass1Output, cancellationToken);
        var targetDurationStr = targetDuration.ToString("0.000", CultureInfo.InvariantCulture);

        var filter =
            $"[0:a]aresample=async=1,volume={voiceVol},asetpts=PTS-STARTPTS[voice];" +
            $"[1:a]aloop=loop=-1:size=0,asetpts=N/SR/TB,volume={musicVol}[musicLoop];" +
            $"[musicLoop]atrim=0:{targetDurationStr},asetpts=PTS-STARTPTS[music];" +
            "[voice][music]amix=inputs=2:duration=first:dropout_transition=3[mixed];" +
            "[mixed]acompressor=threshold=-20dB:ratio=4:attack=5:release=250[compressed];" +
            "[compressed]loudnorm=I=-14:LRA=9:TP=-1.0[final_a]";

        var args = string.Format(
            CultureInfo.InvariantCulture,
            "-y -hide_banner -loglevel error " +
            "-i \"{0}\" -i \"{1}\" " +
            "-filter_complex \"{2}\" " +
            "-map 0:v -map \"[final_a]\" " +
            "-c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a aac " +
            "-t {3} " +
            "\"{4}\"",
            pass1Output, request.MusicPath, filter, targetDurationStr, pass2Output);

        _logger.LogInformation("Pass 2: mixing music {Music} into video.", request.MusicPath);
        await _ffmpeg.RunFfmpegAsync(args, cancellationToken);
        return pass2Output;
    }

    // ------------------------------------------------------------------
    //  Pass 3 — Subscribe GIF overlay (last N seconds)
    // ------------------------------------------------------------------

    private async Task<string> RunPass3OverlayAsync(
        string pass2Output,
        OverlayGifSettings overlay,
        string finalOutputPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(overlay.AssetPath) || !File.Exists(overlay.AssetPath))
        {
            _logger.LogInformation(
                "Pass 3: overlay asset missing or unset, copying pass-2 video to final output.");
            File.Copy(pass2Output, finalOutputPath, overwrite: true);
            return finalOutputPath;
        }

        var totalDuration = await _ffmpeg.GetDurationSecondsAsync(pass2Output, cancellationToken);
        var startSeconds = Math.Max(0d, totalDuration - overlay.TailSeconds);
        var startStr = startSeconds.ToString("0.000", CultureInfo.InvariantCulture);

        // Map Y % → pixel offset from the bottom of the canvas, mirroring
        // the legacy "main_h-overlay_h-90" formula but parameterised.
        var yMargin = (int)Math.Round(CanvasHeight * (1.0 - overlay.YPositionPercent / 100.0));
        if (yMargin < 0) yMargin = 0;

        // Loop count → ffmpeg "loop=loop=N:size=...". For a one-shot GIF
        // the input is left untouched. For -1 (infinite) we add a stream
        // loop so the GIF refills if it ends before the video does.
        var loopPrefix = overlay.LoopCount switch
        {
            -1 => "-stream_loop -1 ",
            0  => string.Empty,
            > 0 => $"-stream_loop {overlay.LoopCount} ",
            _ => string.Empty,
        };

        var filter =
            $"[1:v]format=rgba,setpts=PTS-STARTPTS+{startStr}/TB[gif];" +
            $"[0:v][gif]overlay=(main_w-overlay_w)/2:main_h-overlay_h-{yMargin}:" +
            $"enable='gte(t,{startStr})'[final_v]";

        var args = string.Format(
            CultureInfo.InvariantCulture,
            "-y -hide_banner -loglevel error " +
            "-i \"{0}\" {1}-i \"{2}\" " +
            "-filter_complex \"{3}\" " +
            "-map \"[final_v]\" -map 0:a " +
            "-c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a copy " +
            "\"{4}\"",
            pass2Output, loopPrefix, overlay.AssetPath, filter, finalOutputPath);

        _logger.LogInformation(
            "Pass 3: overlaying {Asset} from t={Start}s onto final output.",
            overlay.AssetPath, startStr);
        await _ffmpeg.RunFfmpegAsync(args, cancellationToken);
        return finalOutputPath;
    }
}
