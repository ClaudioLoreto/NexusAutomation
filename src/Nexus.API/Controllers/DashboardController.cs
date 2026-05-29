using Microsoft.AspNetCore.Mvc;
using Nexus.Core.DTOs;
using Nexus.Core.Interfaces;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IVideoQueueService _queue;

    public DashboardController(IVideoQueueService queue) => _queue = queue;

    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        => Ok(await _queue.GetDashboardSummaryAsync(cancellationToken));
}
