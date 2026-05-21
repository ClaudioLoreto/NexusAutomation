using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;
using Nexus.Data;

namespace Nexus.API.Services;

public class NicheManager : INicheManager
{
    private readonly NexusDbContext _db;
    private readonly ILogger<NicheManager> _logger;

    public NicheManager(NexusDbContext db, ILogger<NicheManager> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<NicheConfigDto> GetNicheConfigAsync(NicheType niche, CancellationToken ct = default)
    {
        var config = await _db.NicheConfigs.FirstOrDefaultAsync(n => n.NicheType == niche, ct)
            ?? throw new InvalidOperationException($"Niche config not found for {niche}");

        return MapToDto(config);
    }

    public async Task<IReadOnlyList<NicheConfigDto>> GetAllNicheConfigsAsync(CancellationToken ct = default)
    {
        var configs = await _db.NicheConfigs.OrderBy(n => n.NicheType).ToListAsync(ct);
        return configs.Select(MapToDto).ToList();
    }

    public async Task UpdateNichePriorityAsync(NicheType niche, int newPriority, CancellationToken ct = default)
    {
        var config = await _db.NicheConfigs.FirstOrDefaultAsync(n => n.NicheType == niche, ct);
        if (config == null) return;

        config.Priority = newPriority;
        config.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated niche {Niche} priority to {Priority}", niche, newPriority);
    }

    private static NicheConfigDto MapToDto(Data.Models.NicheConfig config) => new()
    {
        Id = config.Id,
        NicheType = config.NicheType,
        DisplayName = config.DisplayName,
        ScriptTone = config.ScriptTone,
        VoiceStyle = config.VoiceStyle,
        MusicGenre = config.MusicGenre,
        MusicDirectoryPath = config.MusicDirectoryPath,
        SearchKeywords = config.SearchKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        IsActive = config.IsActive,
        Priority = config.Priority
    };
}
