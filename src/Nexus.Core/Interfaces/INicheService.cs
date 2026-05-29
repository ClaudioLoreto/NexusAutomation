using Nexus.Core.DTOs;
using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public interface INicheService
{
    Task<IReadOnlyList<NicheDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<NicheDto?> GetByTypeAsync(NicheType type, CancellationToken cancellationToken = default);

    Task<NicheDto?> SetActiveAsync(NicheType type, bool isActive, CancellationToken cancellationToken = default);

    Task<NicheDto?> SetQueuePriorityAsync(NicheType type, int queuePriority, CancellationToken cancellationToken = default);
}
