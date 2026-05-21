using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Core.Configuration;
using Nexus.Core.DTOs;
using Nexus.Core.Interfaces;
using Xabe.FFmpeg;

namespace Nexus.Engine.Services;

public class FfmpegVideoRenderer : IVideoRenderer
{
    private readonly EngineOptions _options;
    private readonly ISubtitleGenerator _subtitleGenerator;
    private readonly ILogger<FfmpegVideoRenderer> _logger;
    private readonly Random _random = new();

    public FfmpegVideoRenderer(
        IOptions<EngineOptions> options,
        ISubtitleGenerator subtitleGenerator,
        ILogger<FfmpegVideoRenderer> logger)
    {
        _options = options.Value;
        _subtitleGenerator = subtitleGenerator;
        _logger = logger;

        FFmpeg.SetExecutablesPath(Path.GetDirectoryName(_options.FfmpegPath) ?? "/usr/bin");
    }

    public async Task<RenderResult> RenderVideoAsync(RenderRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Rendering video {VideoId}", request.VideoId);

        try
        {
            Directory.CreateDirectory(_options.OutputDirectory);
            var outputPath = Path.Combine(_options.OutputDirectory, $"{request.VideoId}_final.mp4");

            var subtitlePath = Path.Combine(_options.OutputDirectory, $"{request.VideoId}.ass");
            var mediaInfo = await FFmpeg.GetMediaInfo(request.AudioFilePath, ct);
            var audioDuration = mediaInfo.Duration.TotalSeconds;

            await _subtitleGenerator.GenerateAssSubtitlesAsync(
                request.ScriptText, audioDuration, subtitlePath, ct);

            var musicFile = SelectRandomMusicFile(request.MusicDirectoryPath);

            var zoomPercent = _random.NextDouble() *
                (_options.MicroZoomMaxPercent - _options.MicroZoomMinPercent)
                + _options.MicroZoomMinPercent;
            var zoomFactor = 1.0 + (zoomPercent / 100.0);

            var lutFile = SelectRandomLutFile();

            var filterComplex = BuildFilterComplex(
                zoomFactor,
                lutFile,
                subtitlePath,
                _options.DuckingLevelDb,
                musicFile != null);

            var args = BuildFfmpegArguments(
                request.MediaFilePath,
                request.AudioFilePath,
                musicFile,
                filterComplex,
                outputPath);

            _logger.LogDebug("FFmpeg args: {Args}", args);

            var conversion = FFmpeg.Conversions.New();
            conversion.AddParameter(args);
            conversion.SetOverwriteOutput(true);

            await conversion.Start(ct);

            var outputInfo = await FFmpeg.GetMediaInfo(outputPath, ct);

            _logger.LogInformation("Render complete: {Path} ({Duration:F1}s)",
                outputPath, outputInfo.Duration.TotalSeconds);

            return new RenderResult
            {
                Success = true,
                OutputFilePath = outputPath,
                DurationSeconds = outputInfo.Duration.TotalSeconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rendering failed for video {VideoId}", request.VideoId);
            return new RenderResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private string BuildFilterComplex(
        double zoomFactor,
        string? lutFile,
        string subtitlePath,
        int duckingDb,
        bool hasBgMusic)
    {
        var filters = new List<string>();

        var escapedSubPath = subtitlePath.Replace("\\", "/").Replace(":", "\\:");
        filters.Add($"[0:v]scale=iw*{zoomFactor:F4}:ih*{zoomFactor:F4},crop=1080:1920");

        if (!string.IsNullOrEmpty(lutFile) && File.Exists(lutFile))
        {
            var escapedLut = lutFile.Replace("\\", "/").Replace(":", "\\:");
            filters.Add($"lut3d='{escapedLut}'");
        }

        filters.Add($"ass='{escapedSubPath}'");

        var videoFilterChain = string.Join(",", filters);

        if (hasBgMusic)
        {
            return $"{videoFilterChain}[vout];" +
                   $"[1:a]aformat=sample_fmts=fltp:sample_rates=44100:channel_layouts=stereo[voice];" +
                   $"[2:a]aformat=sample_fmts=fltp:sample_rates=44100:channel_layouts=stereo," +
                   $"volume={duckingDb}dB[music];" +
                   $"[voice][music]amix=inputs=2:duration=first:dropout_transition=2[aout]";
        }

        return $"{videoFilterChain}[vout];[1:a]aformat=sample_fmts=fltp:sample_rates=44100:channel_layouts=stereo[aout]";
    }

    private static string BuildFfmpegArguments(
        string videoInput,
        string audioInput,
        string? musicInput,
        string filterComplex,
        string outputPath)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"-i \"{videoInput}\" ");
        sb.Append($"-i \"{audioInput}\" ");

        if (!string.IsNullOrEmpty(musicInput))
        {
            sb.Append($"-i \"{musicInput}\" ");
        }

        sb.Append($"-filter_complex \"{filterComplex}\" ");
        sb.Append("-map \"[vout]\" -map \"[aout]\" ");
        sb.Append("-c:v libx264 -preset medium -crf 18 ");
        sb.Append("-c:a aac -b:a 192k ");
        sb.Append("-r 30 -shortest ");
        sb.Append($"\"{outputPath}\"");

        return sb.ToString();
    }

    private string? SelectRandomMusicFile(string musicDirectory)
    {
        if (!Directory.Exists(musicDirectory))
        {
            _logger.LogWarning("Music directory not found: {Path}", musicDirectory);
            return null;
        }

        var files = Directory.GetFiles(musicDirectory, "*.mp3")
            .Concat(Directory.GetFiles(musicDirectory, "*.wav"))
            .Concat(Directory.GetFiles(musicDirectory, "*.ogg"))
            .ToArray();

        if (files.Length == 0)
        {
            _logger.LogWarning("No music files found in {Path}", musicDirectory);
            return null;
        }

        return files[_random.Next(files.Length)];
    }

    private string? SelectRandomLutFile()
    {
        if (!Directory.Exists(_options.LutDirectory))
            return null;

        var files = Directory.GetFiles(_options.LutDirectory, "*.cube")
            .Concat(Directory.GetFiles(_options.LutDirectory, "*.3dl"))
            .ToArray();

        return files.Length == 0 ? null : files[_random.Next(files.Length)];
    }
}
