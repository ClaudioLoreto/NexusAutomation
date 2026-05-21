using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexus.Core.Configuration;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;
using Nexus.Data;
using Nexus.Data.Models;

namespace Nexus.Analysis.Services;

public class YouTubeTrendAnalyzer : ITrendAnalyzer, IDisposable
{
    private readonly YouTubeService _youtube;
    private readonly NexusDbContext _db;
    private readonly YouTubeApiOptions _options;
    private readonly ILogger<YouTubeTrendAnalyzer> _logger;

    private static readonly Dictionary<NicheType, string[]> NicheSearchTerms = new()
    {
        [NicheType.Finance] = ["stock market shorts", "finance tips", "investing 2024", "crypto news"],
        [NicheType.TechAndAI] = ["AI news shorts", "tech review", "artificial intelligence", "future technology"],
        [NicheType.LegalAndCourt] = ["courtroom drama", "legal case", "judge ruling", "trial verdict"]
    };

    public YouTubeTrendAnalyzer(
        NexusDbContext db,
        IOptions<YouTubeApiOptions> options,
        ILogger<YouTubeTrendAnalyzer> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;

        _youtube = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = _options.ApiKey,
            ApplicationName = "NexusAutomation"
        });
    }

    public async Task<TrendDto> AnalyzeNicheVelocityAsync(NicheType niche, CancellationToken ct = default)
    {
        _logger.LogInformation("Analyzing view velocity for niche: {Niche}", niche);

        var searchTerms = NicheSearchTerms.GetValueOrDefault(niche, ["shorts"]);
        var totalViewVelocity = 0.0;
        var sampleCount = 0;
        var topVideoIds = new List<string>();

        foreach (var term in searchTerms)
        {
            try
            {
                var searchRequest = _youtube.Search.List("snippet");
                searchRequest.Q = term;
                searchRequest.Type = "video";
                searchRequest.VideoDuration = SearchResource.ListRequest.VideoDurationEnum.Short__;
                searchRequest.Order = SearchResource.ListRequest.OrderEnum.ViewCount;
                searchRequest.MaxResults = _options.MaxResultsPerQuery;
                searchRequest.PublishedAfterDateTimeOffset = DateTimeOffset.UtcNow.AddHours(-_options.ViewVelocityWindowHours);

                var searchResponse = await searchRequest.ExecuteAsync(ct);
                var videoIds = searchResponse.Items
                    .Where(i => i.Id?.VideoId != null)
                    .Select(i => i.Id.VideoId)
                    .ToList();

                if (videoIds.Count == 0) continue;

                var statsRequest = _youtube.Videos.List("statistics");
                statsRequest.Id = string.Join(",", videoIds);
                var statsResponse = await statsRequest.ExecuteAsync(ct);

                foreach (var video in statsResponse.Items)
                {
                    if (video.Statistics?.ViewCount == null) continue;

                    var views = (double)video.Statistics.ViewCount.Value;
                    var velocity = views / _options.ViewVelocityWindowHours;
                    totalViewVelocity += velocity;
                    sampleCount++;
                    topVideoIds.Add(video.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze term '{Term}' for niche {Niche}", term, niche);
            }
        }

        var avgVelocity = sampleCount > 0 ? totalViewVelocity / sampleCount : 0;

        var nicheConfig = await _db.NicheConfigs.FirstOrDefaultAsync(n => n.NicheType == niche, ct);

        var snapshot = new TrendSnapshot
        {
            Id = Guid.NewGuid(),
            Niche = niche,
            ViewVelocity = avgVelocity,
            SampleSize = sampleCount,
            TopVideoIds = string.Join(",", topVideoIds.Take(10)),
            AnalyzedAtUtc = DateTime.UtcNow,
            NicheConfigId = nicheConfig?.Id ?? Guid.Empty
        };

        _db.TrendSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Niche {Niche}: velocity={Velocity:F2} views/hr, samples={Samples}",
            niche, avgVelocity, sampleCount);

        return new TrendDto
        {
            Id = snapshot.Id,
            Niche = niche,
            ViewVelocity = avgVelocity,
            SampleSize = sampleCount,
            AnalyzedAtUtc = snapshot.AnalyzedAtUtc
        };
    }

    public async Task<Dictionary<NicheType, double>> GetAllNicheVelocitiesAsync(CancellationToken ct = default)
    {
        var results = new Dictionary<NicheType, double>();
        foreach (var niche in Enum.GetValues<NicheType>())
        {
            var trend = await AnalyzeNicheVelocityAsync(niche, ct);
            results[niche] = trend.ViewVelocity;
        }
        return results;
    }

    public async Task<NicheType> GetTopPerformingNicheAsync(CancellationToken ct = default)
    {
        var velocities = await GetAllNicheVelocitiesAsync(ct);
        return velocities.OrderByDescending(v => v.Value).First().Key;
    }

    public void Dispose()
    {
        _youtube.Dispose();
    }
}
