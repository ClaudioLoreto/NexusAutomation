using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// Audit row for each render attempt of a <see cref="Video"/>. Records the
/// applied anti-reused-content parameters (micro-zoom factor, LUT path) so
/// that re-renders can be reproduced deterministically.
/// </summary>
public class RenderJob
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;

    [Required, MaxLength(128)]
    public string HangfireJobId { get; set; } = string.Empty;

    [Required]
    public VideoStatus StatusAtStart { get; set; }

    public VideoStatus? StatusAtEnd { get; set; }

    public double? MicroZoomFactor { get; set; }

    [MaxLength(512)]
    public string? AppliedLutPath { get; set; }

    public double? MusicDuckDb { get; set; }

    [MaxLength(8192)]
    public string? FfmpegCommand { get; set; }

    [MaxLength(8192)]
    public string? ErrorMessage { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset? FinishedAtUtc { get; set; }
}
