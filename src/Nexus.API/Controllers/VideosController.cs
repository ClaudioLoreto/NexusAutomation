using Microsoft.AspNetCore.Mvc;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly IVideoQueueService _queue;

    public VideosController(IVideoQueueService queue) => _queue = queue;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VideoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] VideoStatus? status = null,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 500)
            return BadRequest(new { error = "take must be between 1 and 500." });

        var rows = await _queue.GetVideosAsync(status, take, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VideoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _queue.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VideoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Queue(
        [FromBody] QueueVideoRequest body,
        CancellationToken cancellationToken)
    {
        if (body is null)
            return BadRequest(new { error = "Body required." });

        try
        {
            var dto = await _queue.QueueAsync(body, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
