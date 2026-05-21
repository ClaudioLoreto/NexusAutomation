using Nexus.Core.Dtos;
using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public interface ITrendAnalysisService
{
    Task<TrendSnapshotDto> CaptureSnapshotAsync(NicheKey niche, CancellationToken ct = default);

    Task<IReadOnlyDictionary<NicheKey, double>> GetCurrentVelocitiesAsync(CancellationToken ct = default);
}
