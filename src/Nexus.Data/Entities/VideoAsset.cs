using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

/// <summary>
/// One on-disk artifact tied to a <see cref="VideoJob"/> — a Storyblocks
/// clip, the synthesised voiceover, the karaoke ASS file, the final MP4,
/// etc. Replaces the legacy YouTubeAutomation pattern of stuffing all
/// path fields into a single <c>Media</c> row.
/// </summary>
public class VideoAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VideoJobId { get; set; }
    public VideoJob VideoJob { get; set; } = null!;

    public VideoAssetKind Kind { get; set; }

    /// <summary>Absolute path to the artifact on disk.</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Optional MIME type (e.g. <c>video/mp4</c>, <c>audio/mpeg</c>).</summary>
    public string? MediaType { get; set; }

    /// <summary>File size in bytes when known.</summary>
    public long? SizeBytes { get; set; }

    /// <summary>Useful for clips so the assembler can pre-compute total length without ffprobing.</summary>
    public TimeSpan? Duration { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
