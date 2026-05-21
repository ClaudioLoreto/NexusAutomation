using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.API.Jobs;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Data;
using Nexus.Data.Models;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly NexusDbContext _db;
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly ILogger<VideosController> _logger;

    public VideosController(
        NexusDbContext db,
        IBackgroundJobClient backgroundJobs,
        ILogger<VideosController> logger)
    {
        _db = db;
        _backgroundJobs = backgroundJobs;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<VideoDto>>> GetAll(
        [FromQuery] VideoStatus? status = null,
        [FromQuery] NicheType? niche = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.VideoProjects.AsQueryable();

        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);
        if (niche.HasValue)
            query = query.Where(v => v.Niche == niche.Value);

        var videos = await query
            .OrderByDescending(v => v.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VideoDto
            {
                Id = v.Id,
                Title = v.Title,
                Niche = v.Niche,
                Status = v.Status,
                ScriptText = v.ScriptText,
                MediaFilePath = v.MediaFilePath,
                AudioFilePath = v.AudioFilePath,
                OutputFilePath = v.OutputFilePath,
                ErrorMessage = v.ErrorMessage,
                CreatedAtUtc = v.CreatedAtUtc,
                CompletedAtUtc = v.CompletedAtUtc
            })
            .ToListAsync();

        return Ok(videos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VideoDto>> GetById(Guid id)
    {
        var video = await _db.VideoProjects.FindAsync(id);
        if (video == null) return NotFound();

        return Ok(new VideoDto
        {
            Id = video.Id,
            Title = video.Title,
            Niche = video.Niche,
            Status = video.Status,
            ScriptText = video.ScriptText,
            MediaFilePath = video.MediaFilePath,
            AudioFilePath = video.AudioFilePath,
            OutputFilePath = video.OutputFilePath,
            ErrorMessage = video.ErrorMessage,
            CreatedAtUtc = video.CreatedAtUtc,
            CompletedAtUtc = video.CompletedAtUtc
        });
    }

    [HttpPost]
    public async Task<ActionResult<VideoDto>> Create([FromBody] CreateVideoRequest request)
    {
        var nicheConfig = await _db.NicheConfigs
            .FirstOrDefaultAsync(n => n.NicheType == request.Niche);

        if (nicheConfig == null)
            return BadRequest($"Niche config not found for {request.Niche}");

        var video = new VideoProject
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Niche = request.Niche,
            Status = VideoStatus.Pending,
            NicheConfigId = nicheConfig.Id,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.VideoProjects.Add(video);
        await _db.SaveChangesAsync();

        _backgroundJobs.Enqueue<VideoPipelineJob>(
            job => job.ProcessSingleVideo(video.Id));

        _logger.LogInformation("Created video {VideoId} and enqueued for processing", video.Id);

        return CreatedAtAction(nameof(GetById), new { id = video.Id }, new VideoDto
        {
            Id = video.Id,
            Title = video.Title,
            Niche = video.Niche,
            Status = video.Status,
            CreatedAtUtc = video.CreatedAtUtc
        });
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult> Retry(Guid id)
    {
        var video = await _db.VideoProjects.FindAsync(id);
        if (video == null) return NotFound();

        if (video.Status != VideoStatus.ErrorRequiresHuman)
            return BadRequest("Video is not in error state");

        video.Status = VideoStatus.Pending;
        video.ErrorMessage = null;
        video.RetryCount++;
        video.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _backgroundJobs.Enqueue<VideoPipelineJob>(
            job => job.ProcessSingleVideo(video.Id));

        return Ok();
    }
}

public record CreateVideoRequest
{
    public string Title { get; init; } = string.Empty;
    public NicheType Niche { get; init; }
}
