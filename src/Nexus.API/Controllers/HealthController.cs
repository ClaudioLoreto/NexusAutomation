using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.Data;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly NexusDbContext _db;

    public HealthController(NexusDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        service = "Nexus.API",
        status = "ok",
        utc = DateTimeOffset.UtcNow
    });

    [HttpGet("db")]
    public async Task<IActionResult> Db(CancellationToken ct)
    {
        var canConnect = await _db.Database.CanConnectAsync(ct);
        return Ok(new
        {
            canConnect,
            provider = _db.Database.ProviderName
        });
    }
}
