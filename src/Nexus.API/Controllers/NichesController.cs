using Microsoft.AspNetCore.Mvc;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NichesController : ControllerBase
{
    private readonly INicheService _niches;

    public NichesController(INicheService niches) => _niches = niches;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NicheDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await _niches.GetAllAsync(cancellationToken));

    [HttpGet("{type}")]
    [ProducesResponseType(typeof(NicheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(NicheType type, CancellationToken cancellationToken)
    {
        var dto = await _niches.GetByTypeAsync(type, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPatch("{type}/active")]
    [ProducesResponseType(typeof(NicheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(
        NicheType type,
        [FromBody] SetActiveRequest body,
        CancellationToken cancellationToken)
    {
        var dto = await _niches.SetActiveAsync(type, body.IsActive, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPatch("{type}/priority")]
    [ProducesResponseType(typeof(NicheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPriority(
        NicheType type,
        [FromBody] SetPriorityRequest body,
        CancellationToken cancellationToken)
    {
        if (body.QueuePriority < 0)
            return BadRequest(new { error = "QueuePriority must be >= 0." });

        var dto = await _niches.SetQueuePriorityAsync(type, body.QueuePriority, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    public sealed record SetActiveRequest(bool IsActive);
    public sealed record SetPriorityRequest(int QueuePriority);
}
