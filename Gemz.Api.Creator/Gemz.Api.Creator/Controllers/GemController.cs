using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers;

[ApiController]
[Route("api/gems")]
public class GemsController : ControllerBase
{
    private readonly IGemService _gemService;
    private readonly ILogger<GemsController> _logger;

    public GemsController(IGemService gemService, ILogger<GemsController> logger)
    {
        _gemService = gemService;
        _logger = logger;
    }
    
    [HttpPost("getbyid")]
    public async Task<IActionResult> GetById([FromBody] GemIdModel gemIdModel)
    {
        _logger.LogDebug("Entered GetById end point on Gems Api.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _gemService.FetchGemById(gemIdModel, creatorId);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGem([FromBody] GemModel gemModel)
    {
        _logger.LogDebug("Entered CreateGem end point on Gems Api.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _gemService.CreateGem(gemModel, creatorId);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("page")]
    public async Task<IActionResult> FetchGemsInCollection([FromBody] GemsPagingModel gemsPagingModel)
    {
        _logger.LogDebug("Entered FetchGemsInCollection end point on Gems Api.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _gemService.FetchGemsInCollection(gemsPagingModel, creatorId);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }
    
    [HttpPost("publish")]
    public async Task<IActionResult> PublishGem([FromBody] GemIdModel gemIdModel)
    {
        _logger.LogDebug("Entered PublishGem end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _gemService.UpdatePublishedStatusForGem(gemIdModel, creatorId, 1);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("unpublish")]
    public async Task<IActionResult> UnpublishGem([FromBody] GemIdModel gemIdModel)
    {
        _logger.LogDebug("Entered UnpublishGem end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _gemService.UpdatePublishedStatusForGem(gemIdModel, creatorId, 0);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateGem([FromBody] GemUpdateModel gemUpdateModel)
    {
        _logger.LogDebug("Entered UpdateGem end point on Creator API.");

        var creatorId = HttpContext.Items["Account"].ToString();

        var res = await _gemService.UpdateGem(gemUpdateModel, creatorId);

        _logger.LogDebug("Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveGem([FromBody] GemIdModel gemIdModel)
    {
        _logger.LogDebug("Entered ArchiveGem end point on Creator API.");

        var creatorId = HttpContext.Items["Account"].ToString();

        var res = await _gemService.ArchiveGem(gemIdModel, creatorId);

        _logger.LogDebug("Returned from Service function.");
        return Ok(res);
    }
}