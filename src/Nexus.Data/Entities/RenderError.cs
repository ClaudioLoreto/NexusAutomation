using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// A timestamped error row attached to a <see cref="VideoJob"/>. The
/// legacy YouTubeAutomation pipeline only logged via <c>ILogger</c> + email;
/// Nexus persists every failure so the dashboard can render an audit
/// trail and so the queue runner can retry intelligently.
/// </summary>
public class RenderError
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VideoJobId { get; set; }
    public VideoJob VideoJob { get; set; } = null!;

    /// <summary>Pipeline phase active when the failure occurred.</summary>
    public VideoJobPhase PhaseAtFailure { get; set; }

    /// <summary>Exception type or short error code (e.g. "FfmpegExitCode1").</summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>Human-readable error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional full stack trace / FFmpeg stderr tail.</summary>
    public string? Detail { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
