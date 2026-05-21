using Nexus.Core.Enums;

namespace Nexus.Data.Entities;

public class Video
{
    public Guid Id { get; set; }
    public int NicheId { get; set; }
    public Niche Niche { get; set; } = null!;
    public VideoStatus Status { get; set; } = VideoStatus.Pending;
    public string? Title { get; set; }
    public string? ScriptText { get; set; }
    public string? MediaTagsJson { get; set; }
    public string? LocalMediaPath { get; set; }
    public string? OutputPath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Trend> Trends { get; set; } = new List<Trend>();
}
