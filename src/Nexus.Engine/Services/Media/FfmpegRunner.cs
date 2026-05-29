using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nexus.Engine.Services.Media;

/// <summary>
/// Thin process wrapper around <c>ffmpeg.exe</c> / <c>ffprobe.exe</c>.
/// Used by <see cref="FFmpegVideoAssembler"/> so the assembler stays
/// focused on filtergraph composition.
/// </summary>
public sealed class FfmpegRunner
{
    private readonly ILogger<FfmpegRunner> _logger;

    public FfmpegRunner(ILogger<FfmpegRunner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Override path for the <c>ffmpeg</c> executable. Defaults to "ffmpeg"
    /// on PATH.
    /// </summary>
    public string FfmpegExecutable { get; init; } = "ffmpeg";

    /// <summary>
    /// Override path for the <c>ffprobe</c> executable. Defaults to "ffprobe"
    /// on PATH.
    /// </summary>
    public string FfprobeExecutable { get; init; } = "ffprobe";

    /// <summary>
    /// Runs <c>ffmpeg</c> with <paramref name="arguments"/>. Throws when the
    /// process exits non-zero. Stderr is captured into the exception
    /// message so the caller (and the API layer) can surface the FFmpeg
    /// error verbatim instead of a meaningless "exit 1".
    /// </summary>
    public Task RunFfmpegAsync(string arguments, CancellationToken cancellationToken = default)
        => RunAsync(FfmpegExecutable, arguments, captureStdout: false, cancellationToken);

    /// <summary>
    /// Runs <c>ffprobe</c> with <paramref name="arguments"/> and returns
    /// stdout (used to read media duration etc).
    /// </summary>
    public async Task<string> RunFfprobeAsync(string arguments, CancellationToken cancellationToken = default)
    {
        var (stdout, _) = await RunAsync(FfprobeExecutable, arguments, captureStdout: true, cancellationToken);
        return stdout;
    }

    /// <summary>
    /// Probes a media file with ffprobe and returns its duration in seconds.
    /// </summary>
    public async Task<double> GetDurationSecondsAsync(string mediaPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaPath);
        if (!File.Exists(mediaPath))
            throw new FileNotFoundException("Media file not found.", mediaPath);

        var args = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{mediaPath}\"";
        var stdout = await RunFfprobeAsync(args, cancellationToken);
        if (double.TryParse(stdout.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds))
            return seconds;
        throw new InvalidOperationException(
            $"ffprobe returned a non-numeric duration for {mediaPath}: '{stdout}'.");
    }

    private async Task<(string Stdout, string Stderr)> RunAsync(
        string executable,
        string arguments,
        bool captureStdout,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        _logger.LogDebug("Running: {Exe} {Args}", executable, arguments);

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var stdoutSb = new StringBuilder();
        var stderrSb = new StringBuilder();
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            if (captureStdout) stdoutSb.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            stderrSb.AppendLine(e.Data);
        };

        if (!process.Start())
            throw new InvalidOperationException($"Failed to start {executable}.");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var snippet = TakeTail(stderrSb.ToString(), 4000);
            throw new InvalidOperationException(
                $"{executable} exited with code {process.ExitCode}. STDERR tail:\n{snippet}");
        }

        return (stdoutSb.ToString(), stderrSb.ToString());
    }

    private static string TakeTail(string text, int maxChars)
        => string.IsNullOrEmpty(text) || text.Length <= maxChars
            ? text
            : "..." + text[^maxChars..];
}
