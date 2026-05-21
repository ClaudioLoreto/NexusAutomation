using Nexus.Core.Enums;

namespace Nexus.Data.Models;

public class VideoProject
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public NicheType Niche { get; set; }
    public VideoStatus Status { get; set; } = VideoStatus.Pending;

    public string? ScriptText { get; set; }
    public string? SsmlText { get; set; }
    public string? MediaFilePath { get; set; }
    public string? MediaTags { get; set; }
    public string? AudioFilePath { get; set; }
    public string? SubtitleFilePath { get; set; }
    public string? OutputFilePath { get; set; }
    public double? AudioDurationSeconds { get; set; }
    public double? VideoDurationSeconds { get; set; }

    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public Guid NicheConfigId { get; set; }
    public NicheConfig NicheConfig { get; set; } = null!;
}
