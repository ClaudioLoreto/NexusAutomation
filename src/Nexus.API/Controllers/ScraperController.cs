using Microsoft.AspNetCore.Mvc;
using Nexus.Core.Interfaces;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
    private readonly IStoryblocksScraper _scraper;

    public ScraperController(IStoryblocksScraper scraper) => _scraper = scraper;

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate(CancellationToken cancellationToken)
    {
        var ok = await _scraper.EnsureAuthenticatedAsync(cancellationToken);
        return Ok(new { authenticated = ok });
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int max = 3,
        CancellationToken cancellationToken = default)
    {
        var results = await _scraper.SearchAndDownloadAsync(query, max, cancellationToken);
        return Ok(results);
    }
}
