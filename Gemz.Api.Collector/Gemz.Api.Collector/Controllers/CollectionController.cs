using Gemz.Api.Collector.Middleware;
using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers;

[ApiController]
[Route("api/collections")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    private readonly ILogger<CollectionController> _logger;

    public CollectionController(ICollectionService collectionService, ILogger<CollectionController> logger)
    {
        _collectionService = collectionService;
        _logger = logger;
    }
    
    [AllowUnauthorized]
    [HttpPost("all")]
    public async Task<IActionResult> GetAllCollectionsForCreator([FromBody] CreatorIdModel creatorIdModel)
    {
        _logger.LogDebug("Entered GetAllCollectionsForCreator end point.");
        
        var res = await _collectionService.GetAllCollectionsForCreator(creatorIdModel);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }
    
    [AllowUnauthorized]
    [HttpPost("page")]
    public async Task<IActionResult> GetPageOfCollectionsForCreator([FromBody] CollectionPagingModel collectionPaging)
    {
        _logger.LogDebug("Entered GetPageOfCollectionsForCreator end point on CollectorApi.");
        
        var res = await _collectionService.GetPagedCollectionsForCreator(collectionPaging);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [AllowUnauthorized]
    [HttpPost("collection")]
    public async Task<IActionResult> GetCollectionForCreator(
        [FromBody] CollectionIdModel collectionIdModel)
    {
        _logger.LogDebug("Entered GetCollectionForCreator end point on CollectorApi.");
        
        var res = await _collectionService.GetSingleCollection(collectionIdModel);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

}