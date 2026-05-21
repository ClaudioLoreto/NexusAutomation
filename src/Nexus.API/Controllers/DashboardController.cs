using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Data;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly NexusDbContext _db;

    public DashboardController(NexusDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var videos = await _db.VideoProjects.ToListAsync();

        var latestTrends = await _db.TrendSnapshots
            .GroupBy(t => t.Niche)
            .Select(g => new { Niche = g.Key, Velocity = g.OrderByDescending(t => t.AnalyzedAtUtc).First().ViewVelocity })
            .ToListAsync();

        var velocities = latestTrends.ToDictionary(t => t.Niche.ToString(), t => t.Velocity);
        var topNiche = latestTrends.OrderByDescending(t => t.Velocity).FirstOrDefault()?.Niche.ToString() ?? "N/A";

        return Ok(new DashboardStatsDto
        {
            TotalVideos = videos.Count,
            PendingVideos = videos.Count(v => v.Status == VideoStatus.Pending),
            CompletedVideos = videos.Count(v => v.Status == VideoStatus.Completed),
            ErrorVideos = videos.Count(v => v.Status == VideoStatus.ErrorRequiresHuman),
            RenderingVideos = videos.Count(v => v.Status == VideoStatus.Rendering),
            NicheVelocities = velocities,
            TopNiche = topNiche
        });
    }
}
