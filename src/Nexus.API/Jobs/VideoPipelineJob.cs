using Nexus.Core.Interfaces;

namespace Nexus.API.Jobs;

public class VideoPipelineJob
{
    private readonly IVideoPipelineOrchestrator _orchestrator;
    private readonly ILogger<VideoPipelineJob> _logger;

    public VideoPipelineJob(
        IVideoPipelineOrchestrator orchestrator,
        ILogger<VideoPipelineJob> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task ProcessPendingVideos()
    {
        _logger.LogInformation("Hangfire: Processing pending videos...");
        await _orchestrator.ProcessPendingVideosAsync();
    }

    public async Task RebalanceNicheQueue()
    {
        _logger.LogInformation("Hangfire: Rebalancing niche queue...");
        await _orchestrator.RebalanceQueueAsync();
    }

    public async Task ProcessSingleVideo(Guid videoId)
    {
        _logger.LogInformation("Hangfire: Processing video {VideoId}", videoId);
        await _orchestrator.ProcessVideoAsync(videoId);
    }
}
