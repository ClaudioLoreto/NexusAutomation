namespace Nexus.Core.Interfaces;

public interface IVideoPipelineOrchestrator
{
    Task ProcessVideoAsync(Guid videoId, CancellationToken ct = default);
    Task ProcessPendingVideosAsync(CancellationToken ct = default);
    Task RebalanceQueueAsync(CancellationToken ct = default);
}
