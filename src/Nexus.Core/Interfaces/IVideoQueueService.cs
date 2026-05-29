using Nexus.Core.DTOs;
using Nexus.Core.Enums;

namespace Nexus.Core.Interfaces;

public interface IVideoQueueService
{
    Task<IReadOnlyList<VideoDto>> GetVideosAsync(
        VideoStatus? status = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<VideoDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VideoDto> QueueAsync(QueueVideoRequest request, CancellationToken cancellationToken = default);

    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
