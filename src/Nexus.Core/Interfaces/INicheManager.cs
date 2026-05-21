using Nexus.Core.DTOs;
using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public interface INicheManager
{
    Task<NicheConfigDto> GetNicheConfigAsync(NicheType niche, CancellationToken ct = default);
    Task<IReadOnlyList<NicheConfigDto>> GetAllNicheConfigsAsync(CancellationToken ct = default);
    Task UpdateNichePriorityAsync(NicheType niche, int newPriority, CancellationToken ct = default);
}
