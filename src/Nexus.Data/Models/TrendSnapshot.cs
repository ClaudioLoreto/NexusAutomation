using Nexus.Core.Enums;

namespace Nexus.Data.Models;

public class TrendSnapshot
{
    public Guid Id { get; set; }
    public NicheType Niche { get; set; }

    /// <summary>Views per hour averaged across sampled Shorts.</summary>
    public double ViewVelocity { get; set; }

    public int SampleSize { get; set; }
    public string? TopVideoIds { get; set; }
    public DateTime AnalyzedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid NicheConfigId { get; set; }
    public NicheConfig NicheConfig { get; set; } = null!;
}
