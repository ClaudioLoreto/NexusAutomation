using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public interface INichePriorityService
{
    /// <summary>
    /// Returns niches ordered by priority (highest first) based on View Velocity ratios.
    /// </summary>
    Task<IReadOnlyList<NicheType>> GetPrioritizedNichesAsync(CancellationToken cancellationToken = default);
}
