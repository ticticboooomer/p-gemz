using Gemz.Api.Collector.Middleware;
using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers;

[ApiController]
[Route("api/stores")]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ILogger<StoreController> _logger;

    public StoreController(IStoreService storeService, ILogger<StoreController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    [AllowUnauthorized]
    [HttpPost("storecheck")]
    public async Task<IActionResult> StoreCheck([FromBody]StoreTagModel storeTagModel)
    {
        _logger.LogDebug("Entered StoreCheck end point on CollectorApi.");
        
        var res = await _storeService.CheckStoreIsValid(storeTagModel);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [AllowUnauthorized]
    [HttpPost("storefront")]
    public async Task<IActionResult> FetchCreatorStoreFront([FromBody]StoreTagModel storeTagModel)
    {
        _logger.LogDebug("Entered StoreCheck end point on CollectorApi.");
        
        var res = await _storeService.FetchCreatorStoreFront(storeTagModel);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [AllowUnauthorized]
    [HttpPost("livestores")]
    public async Task<IActionResult> FetchLiveStoresList()
    {
        _logger.LogDebug("Entered FetchLiveStoresList end point on CollectorApi.");

        var res = await _storeService.FetchLiveStoresList();

        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [AllowUnauthorized]
    [HttpPost("livestorespage")]
    public async Task<IActionResult> FetchLiveStoresListPaged([FromBody] LiveStoresPagedInputModel liveStoresPagedInputModel)
    {
        _logger.LogDebug("Entered FetchLiveStoresListPaged end point on CollectorApi.");

        var res = await _storeService.FetchLiveStoresPage(liveStoresPagedInputModel);

        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

}