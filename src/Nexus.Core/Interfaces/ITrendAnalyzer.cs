using Nexus.Core.DTOs;
using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public interface ITrendAnalyzer
{
    Task<TrendDto> AnalyzeNicheVelocityAsync(NicheType niche, CancellationToken ct = default);
    Task<Dictionary<NicheType, double>> GetAllNicheVelocitiesAsync(CancellationToken ct = default);
    Task<NicheType> GetTopPerformingNicheAsync(CancellationToken ct = default);
}
