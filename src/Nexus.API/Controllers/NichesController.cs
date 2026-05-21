using Microsoft.AspNetCore.Mvc;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NichesController : ControllerBase
{
    private readonly INicheManager _nicheManager;
    private readonly ITrendAnalyzer _trendAnalyzer;

    public NichesController(INicheManager nicheManager, ITrendAnalyzer trendAnalyzer)
    {
        _nicheManager = nicheManager;
        _trendAnalyzer = trendAnalyzer;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NicheConfigDto>>> GetAll()
    {
        var niches = await _nicheManager.GetAllNicheConfigsAsync();
        return Ok(niches);
    }

    [HttpGet("{niche}")]
    public async Task<ActionResult<NicheConfigDto>> GetByNiche(NicheType niche)
    {
        var config = await _nicheManager.GetNicheConfigAsync(niche);
        return Ok(config);
    }

    [HttpGet("velocities")]
    public async Task<ActionResult<Dictionary<NicheType, double>>> GetVelocities()
    {
        var velocities = await _trendAnalyzer.GetAllNicheVelocitiesAsync();
        return Ok(velocities);
    }

    [HttpGet("top")]
    public async Task<ActionResult<NicheType>> GetTopNiche()
    {
        var top = await _trendAnalyzer.GetTopPerformingNicheAsync();
        return Ok(top);
    }
}
