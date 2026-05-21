using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.Data;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly NexusDbContext _db;

    public HealthController(NexusDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var nicheCount = await _db.Niches.CountAsync(cancellationToken);
        return Ok(new
        {
            status = "ok",
            service = "Nexus-Shorts-Engine",
            nichesSeeded = nicheCount,
            databaseConfigured = !string.IsNullOrWhiteSpace(
                HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("PostgreSQL"))
        });
    }
}
