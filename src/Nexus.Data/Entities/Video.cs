using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// Aggregate root for a single generated YouTube Short. Moves through the
/// canonical state machine described in CLAUDE.md.
/// </summary>
public class Video
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public int NicheId { get; set; }
    public Niche Niche { get; set; } = null!;

    [Required]
    public VideoStatus Status { get; set; } = VideoStatus.Pending;

    [MaxLength(256)]
    public string? Title { get; set; }

    [MaxLength(4096)]
    public string? Hook { get; set; }

    [MaxLength(16384)]
    public string? BodySsml { get; set; }

    [MaxLength(1024)]
    public string? CallToAction { get; set; }

    /// <summary>
    /// Comma-separated hashtags. We use a string column rather than a child
    /// table because hashtags are write-once per video.
    /// </summary>
    [MaxLength(1024)]
    public string? HashtagsCsv { get; set; }

    [MaxLength(2048)]
    public string? VoiceOverFilePath { get; set; }

    [MaxLength(2048)]
    public string? SubtitleFilePath { get; set; }

    [MaxLength(2048)]
    public string? BackgroundMusicFilePath { get; set; }

    [MaxLength(2048)]
    public string? FinalRenderFilePath { get; set; }

    [MaxLength(64)]
    public string? YouTubeVideoId { get; set; }

    /// <summary>
    /// Captured at the moment the Analysis worker locked in this video's
    /// niche. Used after-the-fact to attribute performance back to the
    /// original signal.
    /// </summary>
    public double? CapturedViewVelocity { get; set; }

    [MaxLength(8192)]
    public string? LastError { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    public ICollection<RenderJob> RenderJobs { get; set; } = new List<RenderJob>();
}
