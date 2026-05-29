using Nexus.Core.Enums;

namespace Nexus.Core.DTOs;

public sealed record DashboardSummaryDto(
    int TotalVideos,
    IReadOnlyDictionary<VideoStatus, int> CountByStatus,
    IReadOnlyList<NicheDto> Niches,
    DateTime GeneratedAtUtc);
