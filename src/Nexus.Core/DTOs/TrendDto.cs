using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public record TrendDto
{
    public Guid Id { get; init; }
    public NicheType Niche { get; init; }
    public double ViewVelocity { get; init; }
    public int SampleSize { get; init; }
    public DateTime AnalyzedAtUtc { get; init; }
}
