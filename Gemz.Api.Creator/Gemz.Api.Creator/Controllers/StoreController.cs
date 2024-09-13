using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers;

[ApiController]
[Route("api/stores")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ILogger<CollectionController> _logger;

    public StoreController(IStoreService storeService, ILogger<CollectionController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }
    
    [HttpPost("edit")]
    public async Task<IActionResult> EditOrInsertStore([FromBody] StoreUpsertModel storeUpsertModel)
    {
        _logger.LogDebug("Entered EditOrInsertStore end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _storeService.EditOrInsertStoreDetails(storeUpsertModel, creatorId);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("store")]
    public async Task<IActionResult> FetchStoreDetails()
    {
        _logger.LogDebug("Entered FetchStoreDetails end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _storeService.FetchCreatorStoreDetails(creatorId);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("tagcheck")]
    public async Task<IActionResult> TagCheck([FromBody]TagWordModel tagWordModel)
    {
        _logger.LogDebug("Entered TagCheck end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _storeService.CheckTagWordAvailable(creatorId, tagWordModel);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }
}