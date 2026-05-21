using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;
using Nexus.Data;
using Nexus.Data.Models;

namespace Nexus.API.Services;

public class VideoPipelineOrchestrator : IVideoPipelineOrchestrator
{
    private readonly NexusDbContext _db;
    private readonly ITrendAnalyzer _trendAnalyzer;
    private readonly IMediaScraper _mediaScraper;
    private readonly IScriptGenerator _scriptGenerator;
    private readonly ITtsProvider _ttsProvider;
    private readonly IVideoRenderer _videoRenderer;
    private readonly INicheManager _nicheManager;
    private readonly ILogger<VideoPipelineOrchestrator> _logger;

    public VideoPipelineOrchestrator(
        NexusDbContext db,
        ITrendAnalyzer trendAnalyzer,
        IMediaScraper mediaScraper,
        IScriptGenerator scriptGenerator,
        ITtsProvider ttsProvider,
        IVideoRenderer videoRenderer,
        INicheManager nicheManager,
        ILogger<VideoPipelineOrchestrator> logger)
    {
        _db = db;
        _trendAnalyzer = trendAnalyzer;
        _mediaScraper = mediaScraper;
        _scriptGenerator = scriptGenerator;
        _ttsProvider = ttsProvider;
        _videoRenderer = videoRenderer;
        _nicheManager = nicheManager;
        _logger = logger;
    }

    public async Task ProcessVideoAsync(Guid videoId, CancellationToken ct = default)
    {
        var video = await _db.VideoProjects
            .Include(v => v.NicheConfig)
            .FirstOrDefaultAsync(v => v.Id == videoId, ct);

        if (video == null)
        {
            _logger.LogError("Video {VideoId} not found", videoId);
            return;
        }

        try
        {
            switch (video.Status)
            {
                case VideoStatus.Pending:
                    await RunTrendAnalysisAsync(video, ct);
                    break;
                case VideoStatus.TrendAnalyzed:
                    await RunScriptingAsync(video, ct);
                    break;
                case VideoStatus.Scripting:
                    await RunMediaDownloadAsync(video, ct);
                    break;
                case VideoStatus.MediaDownloaded:
                    await RunRenderingAsync(video, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline failed for video {VideoId} at status {Status}",
                videoId, video.Status);
            video.Status = VideoStatus.ErrorRequiresHuman;
            video.ErrorMessage = ex.Message;
            video.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task RunTrendAnalysisAsync(VideoProject video, CancellationToken ct)
    {
        _logger.LogInformation("Step 1: Trend analysis for video {VideoId}", video.Id);
        await _trendAnalyzer.AnalyzeNicheVelocityAsync(video.Niche, ct);

        video.Status = VideoStatus.TrendAnalyzed;
        video.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await ProcessVideoAsync(video.Id, ct);
    }

    private async Task RunScriptingAsync(VideoProject video, CancellationToken ct)
    {
        _logger.LogInformation("Step 2: Scripting for video {VideoId}", video.Id);

        var nicheConfig = video.NicheConfig ?? await _db.NicheConfigs
            .FirstAsync(n => n.NicheType == video.Niche, ct);

        var tags = video.MediaTags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (tags.Length == 0)
            tags = nicheConfig.SearchKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var scriptResult = await _scriptGenerator.GenerateScriptAsync(new ScriptGenerationRequest
        {
            VideoId = video.Id,
            Niche = video.Niche,
            Tone = nicheConfig.ScriptTone,
            MediaTags = tags,
            TargetDurationSeconds = 55
        }, ct);

        if (!scriptResult.Success)
            throw new InvalidOperationException($"Script generation failed: {scriptResult.ErrorMessage}");

        video.ScriptText = scriptResult.ScriptText;
        video.SsmlText = scriptResult.SsmlText;
        video.Status = VideoStatus.Scripting;
        video.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await ProcessVideoAsync(video.Id, ct);
    }

    private async Task RunMediaDownloadAsync(VideoProject video, CancellationToken ct)
    {
        _logger.LogInformation("Step 3: Media download for video {VideoId}", video.Id);

        var nicheConfig = video.NicheConfig ?? await _db.NicheConfigs
            .FirstAsync(n => n.NicheType == video.Niche, ct);

        var keywords = nicheConfig.SearchKeywords
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = await _mediaScraper.DownloadVerticalVideoAsync(video.Niche, keywords, ct);

        if (!result.Success)
        {
            if (result.RequiresHumanIntervention)
            {
                video.Status = VideoStatus.ErrorRequiresHuman;
                video.ErrorMessage = result.ErrorMessage;
            }
            else
            {
                throw new InvalidOperationException($"Media download failed: {result.ErrorMessage}");
            }
            video.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return;
        }

        video.MediaFilePath = result.FilePath;
        video.MediaTags = string.Join(",", result.Tags);
        video.Status = VideoStatus.MediaDownloaded;
        video.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var ttsResult = await _ttsProvider.SynthesizeSpeechAsync(new TtsRequest
        {
            VideoId = video.Id,
            SsmlText = video.SsmlText ?? video.ScriptText ?? "",
            VoiceStyle = nicheConfig.VoiceStyle
        }, ct);

        if (!ttsResult.Success)
            throw new InvalidOperationException($"TTS failed: {ttsResult.ErrorMessage}");

        video.AudioFilePath = ttsResult.AudioFilePath;
        video.AudioDurationSeconds = ttsResult.DurationSeconds;
        await _db.SaveChangesAsync(ct);

        await ProcessVideoAsync(video.Id, ct);
    }

    private async Task RunRenderingAsync(VideoProject video, CancellationToken ct)
    {
        _logger.LogInformation("Step 4: Rendering for video {VideoId}", video.Id);
        video.Status = VideoStatus.Rendering;
        video.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var nicheConfig = video.NicheConfig ?? await _db.NicheConfigs
            .FirstAsync(n => n.NicheType == video.Niche, ct);

        var renderResult = await _videoRenderer.RenderVideoAsync(new RenderRequest
        {
            VideoId = video.Id,
            MediaFilePath = video.MediaFilePath!,
            AudioFilePath = video.AudioFilePath!,
            ScriptText = video.ScriptText!,
            Niche = video.Niche,
            MusicGenre = nicheConfig.MusicGenre,
            MusicDirectoryPath = nicheConfig.MusicDirectoryPath
        }, ct);

        if (!renderResult.Success)
            throw new InvalidOperationException($"Rendering failed: {renderResult.ErrorMessage}");

        video.OutputFilePath = renderResult.OutputFilePath;
        video.VideoDurationSeconds = renderResult.DurationSeconds;
        video.Status = VideoStatus.Completed;
        video.CompletedAtUtc = DateTime.UtcNow;
        video.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Video {VideoId} completed: {Path}", video.Id, renderResult.OutputFilePath);
    }

    public async Task ProcessPendingVideosAsync(CancellationToken ct = default)
    {
        var pendingVideos = await _db.VideoProjects
            .Where(v => v.Status == VideoStatus.Pending ||
                        v.Status == VideoStatus.TrendAnalyzed ||
                        v.Status == VideoStatus.Scripting ||
                        v.Status == VideoStatus.MediaDownloaded)
            .OrderBy(v => v.CreatedAtUtc)
            .Take(5)
            .Select(v => v.Id)
            .ToListAsync(ct);

        _logger.LogInformation("Processing {Count} pending videos", pendingVideos.Count);

        foreach (var videoId in pendingVideos)
        {
            await ProcessVideoAsync(videoId, ct);
        }
    }

    public async Task RebalanceQueueAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Rebalancing niche queue priorities...");

        var velocities = await _trendAnalyzer.GetAllNicheVelocitiesAsync(ct);
        if (velocities.Count == 0) return;

        var maxVelocity = velocities.Values.Max();
        var topNiche = velocities.First(v => Math.Abs(v.Value - maxVelocity) < 0.01).Key;

        foreach (var (niche, velocity) in velocities)
        {
            if (niche == topNiche) continue;

            if (maxVelocity > velocity * 3)
            {
                _logger.LogInformation(
                    "Niche {TopNiche} outperforms {Niche} by >{Ratio}% — reprioritizing",
                    topNiche, niche, 200);

                await _nicheManager.UpdateNichePriorityAsync(topNiche, 10, ct);
                await _nicheManager.UpdateNichePriorityAsync(niche, 1, ct);
            }
        }
    }
}
