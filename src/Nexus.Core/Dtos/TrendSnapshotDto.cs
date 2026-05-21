using Nexus.Core.Enums;

namespace Nexus.Core.Dtos;

/// <summary>
/// Snapshot of competitor performance for a niche, taken by the Analysis
/// worker. <see cref="ViewVelocity"/> is expressed in views per hour, computed
/// over the rolling 24h window for each tracked competitor Short and then
/// aggregated (mean) across the niche.
/// </summary>
public sealed record TrendSnapshotDto(
    NicheKey Niche,
    DateTimeOffset CapturedAtUtc,
    double ViewVelocity,
    int SampleSize,
    IReadOnlyList<string> TopKeywords);
