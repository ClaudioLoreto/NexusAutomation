using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// A physical media file associated with a <see cref="Video"/>: Storyblocks
/// clip, background music track, generated voice-over, etc.
/// </summary>
public class MediaAsset
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;

    [Required]
    public MediaAssetKind Kind { get; set; }

    [Required, MaxLength(2048)]
    public string LocalFilePath { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? SourceId { get; set; }

    [MaxLength(2048)]
    public string? SourceUrl { get; set; }

    /// <summary>Comma-separated tags as returned by the source provider.</summary>
    [MaxLength(2048)]
    public string? TagsCsv { get; set; }

    public int? WidthPx { get; set; }
    public int? HeightPx { get; set; }
    public TimeSpan? Duration { get; set; }
    public long? SizeBytes { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
