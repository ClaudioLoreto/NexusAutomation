using Microsoft.EntityFrameworkCore;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;
using Nexus.Data.Entities;

namespace Nexus.Data.Services;

internal sealed class NicheService : INicheService
{
    private readonly NexusDbContext _db;

    public NicheService(NexusDbContext db) => _db = db;

    public async Task<IReadOnlyList<NicheDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.Niches
            .AsNoTracking()
            .OrderByDescending(n => n.QueuePriority)
            .ThenBy(n => n.Name)
            .Select(n => new
            {
                Niche = n,
                VideoCount = _db.Videos.Count(v => v.NicheId == n.Id)
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => Map(r.Niche, r.VideoCount)).ToList();
    }

    public async Task<NicheDto?> GetByTypeAsync(NicheType type, CancellationToken cancellationToken = default)
    {
        var niche = await _db.Niches.AsNoTracking().FirstOrDefaultAsync(n => n.Type == type, cancellationToken);
        if (niche is null)
            return null;
        var count = await _db.Videos.CountAsync(v => v.NicheId == niche.Id, cancellationToken);
        return Map(niche, count);
    }

    public async Task<NicheDto?> SetActiveAsync(NicheType type, bool isActive, CancellationToken cancellationToken = default)
    {
        var niche = await _db.Niches.FirstOrDefaultAsync(n => n.Type == type, cancellationToken);
        if (niche is null)
            return null;
        niche.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);
        var count = await _db.Videos.CountAsync(v => v.NicheId == niche.Id, cancellationToken);
        return Map(niche, count);
    }

    public async Task<NicheDto?> SetQueuePriorityAsync(NicheType type, int queuePriority, CancellationToken cancellationToken = default)
    {
        if (queuePriority < 0)
            throw new ArgumentOutOfRangeException(nameof(queuePriority), "QueuePriority must be >= 0.");

        var niche = await _db.Niches.FirstOrDefaultAsync(n => n.Type == type, cancellationToken);
        if (niche is null)
            return null;
        niche.QueuePriority = queuePriority;
        await _db.SaveChangesAsync(cancellationToken);
        var count = await _db.Videos.CountAsync(v => v.NicheId == niche.Id, cancellationToken);
        return Map(niche, count);
    }

    private static NicheDto Map(Niche n, int videoCount) =>
        new(
            n.Id,
            n.Type,
            n.Name,
            n.LanguageCode,
            n.ScriptTone,
            n.TargetWordCount,
            n.MaxWords,
            n.TtsVoice,
            n.TtsSpeed,
            n.ElevenLabsVoiceId,
            n.MusicDirectory,
            n.KaraokeFontFamily,
            n.KaraokeFontSize,
            n.KaraokeHighlightFontSize,
            n.KaraokeFillColor,
            n.KaraokeHighlightColor,
            n.KaraokeOutlineColor,
            n.KaraokeBackgroundColor,
            n.KaraokeYPositionPercent,
            n.OverlayGifPath,
            n.OverlayGifPositionPercent,
            n.OverlayGifTailSeconds,
            n.OverlayGifLoopCount,
            n.AdditionalScriptInstructions,
            n.IsActive,
            n.QueuePriority,
            videoCount);
}
