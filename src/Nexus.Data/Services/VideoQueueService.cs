using Microsoft.EntityFrameworkCore;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;
using Nexus.Data.Entities;

namespace Nexus.Data.Services;

internal sealed class VideoQueueService : IVideoQueueService
{
    private readonly NexusDbContext _db;

    public VideoQueueService(NexusDbContext db) => _db = db;

    public async Task<IReadOnlyList<VideoDto>> GetVideosAsync(
        VideoStatus? status = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(take), "take must be between 1 and 500.");

        IQueryable<Video> query = _db.Videos.AsNoTracking().Include(v => v.Niche);
        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);

        var rows = await query
            .OrderByDescending(v => v.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<VideoDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var video = await _db.Videos
            .AsNoTracking()
            .Include(v => v.Niche)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        return video is null ? null : Map(video);
    }

    public async Task<VideoDto> QueueAsync(QueueVideoRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var niche = await _db.Niches.FirstOrDefaultAsync(n => n.Type == request.NicheType, cancellationToken)
            ?? throw new InvalidOperationException($"Niche {request.NicheType} is not seeded.");

        if (!niche.IsActive)
            throw new InvalidOperationException($"Niche {niche.Name} is currently inactive — activate it before queuing.");

        var tagsJson = string.IsNullOrWhiteSpace(request.StoryblocksQuery)
            ? null
            : System.Text.Json.JsonSerializer.Serialize(new[] { request.StoryblocksQuery });

        var video = new Video
        {
            Id = Guid.NewGuid(),
            NicheId = niche.Id,
            Status = VideoStatus.Pending,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            MediaTagsJson = tagsJson,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Videos.Add(video);
        await _db.SaveChangesAsync(cancellationToken);

        video.Niche = niche;
        return Map(video);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var statusCounts = await _db.Videos
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var countByStatus = Enum.GetValues<VideoStatus>()
            .ToDictionary(s => s, s => statusCounts.FirstOrDefault(r => r.Status == s)?.Count ?? 0);

        var total = countByStatus.Values.Sum();

        var nicheRows = await _db.Niches
            .AsNoTracking()
            .OrderByDescending(n => n.QueuePriority)
            .ThenBy(n => n.Name)
            .Select(n => new
            {
                Niche = n,
                VideoCount = _db.Videos.Count(v => v.NicheId == n.Id)
            })
            .ToListAsync(cancellationToken);

        var niches = nicheRows
            .Select(r => new NicheDto(
                r.Niche.Id,
                r.Niche.Type,
                r.Niche.Name,
                r.Niche.LanguageCode,
                r.Niche.ScriptTone,
                r.Niche.TargetWordCount,
                r.Niche.MaxWords,
                r.Niche.TtsVoice,
                r.Niche.TtsSpeed,
                r.Niche.ElevenLabsVoiceId,
                r.Niche.MusicDirectory,
                r.Niche.KaraokeFontFamily,
                r.Niche.KaraokeFontSize,
                r.Niche.KaraokeHighlightFontSize,
                r.Niche.KaraokeFillColor,
                r.Niche.KaraokeHighlightColor,
                r.Niche.KaraokeOutlineColor,
                r.Niche.KaraokeBackgroundColor,
                r.Niche.KaraokeYPositionPercent,
                r.Niche.OverlayGifPath,
                r.Niche.OverlayGifPositionPercent,
                r.Niche.OverlayGifTailSeconds,
                r.Niche.OverlayGifLoopCount,
                r.Niche.AdditionalScriptInstructions,
                r.Niche.IsActive,
                r.Niche.QueuePriority,
                r.VideoCount))
            .ToList();

        return new DashboardSummaryDto(total, countByStatus, niches, DateTime.UtcNow);
    }

    private static VideoDto Map(Video v) =>
        new(
            v.Id,
            v.Niche.Type,
            v.Status,
            v.Title,
            v.ScriptText,
            v.OutputPath,
            v.CreatedAtUtc,
            v.UpdatedAtUtc);
}
