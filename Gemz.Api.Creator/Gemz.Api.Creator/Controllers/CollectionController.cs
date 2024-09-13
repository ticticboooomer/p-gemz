using Gemz.Api.Creator.Middleware;
using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers;

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

    [HttpPost("create")]
    public async Task<IActionResult> CreateCollection([FromBody] CollectionModel collection)
    {
        _logger.LogDebug("Entered Create CreateCollection end point on Creator API.");

        var creatorId = HttpContext.Items["Account"].ToString();

        var res = await _collectionService.CreateCollection(collection, creatorId);

        _logger.LogDebug("Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("page")]
    public async Task<IActionResult> GetPage([FromBody] CollectionPagingModel collectionPagingData)
    {
        _logger.LogDebug("Entered GetPage end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _collectionService.FetchPageOfCreatorCollections(creatorId, collectionPagingData);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("getbyid")]
    public async Task<IActionResult> GetById([FromBody] CollectionIdModel collectionIdModel)
    {
        _logger.LogDebug("Entered GetById end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _collectionService.FetchCollectionById(collectionIdModel, creatorId);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [LimitedAccess]
    [HttpPost("publish")]
    public async Task<IActionResult> PublishCollection([FromBody] CollectionIdModel collectionIdModel)
    {
        _logger.LogDebug("Entered PublishCollection end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _collectionService.UpdatePublishedStatusForCollection(collectionIdModel, creatorId, 1);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [LimitedAccess]
    [HttpPost("unpublish")]
    public async Task<IActionResult> UnpublishCollection([FromBody] CollectionIdModel collectionIdModel)
    {
        _logger.LogDebug("Entered UnpublishCollection end point on CreatorApi.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _collectionService.UpdatePublishedStatusForCollection(collectionIdModel, creatorId, 0);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateCollection([FromBody] CollectionUpdateModel collection)
    {
        _logger.LogDebug("Entered UpdateCollection end point on Creator API.");

        var creatorId = HttpContext.Items["Account"].ToString();

        var res = await _collectionService.UpdateCollection(collection, creatorId);

        _logger.LogDebug("Returned from Service function.");
        return Ok(res);
    }
        
    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveCollection([FromBody] CollectionIdModel collectionIdModel)
    {
        _logger.LogDebug("Entered ArchiveCollection end point on Creator API.");

        var creatorId = HttpContext.Items["Account"].ToString();

        var res = await _collectionService.ArchiveCollection(collectionIdModel, creatorId);

        _logger.LogDebug("Returned from Service function.");
        return Ok(res);
    }
}
