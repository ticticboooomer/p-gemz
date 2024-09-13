using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchasesService _purchasesService;
        private readonly ILogger<PurchasesController> _logger;

        public PurchasesController(IPurchasesService purchasesService, ILogger<PurchasesController> logger)
        {
            _purchasesService = purchasesService;
            _logger = logger;
        }

        [HttpPost("stores")]
        public async Task<IActionResult> FetchStoresContainingPurchases()
        {
            _logger.LogDebug("Entered FetchStoresContainingPurchases end point.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _purchasesService.FetchStoresContainingPurchases(collectorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("collections")]
        public async Task<IActionResult> FetchCollectionPurchasesdFromForOneStore([FromBody] PurchCollectionsInputModel purchCollectionsInputModel)
        {
            _logger.LogDebug("Entered FetchCollectionPurchasesdFromForOneStore end point.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _purchasesService.FetchCollectionsOneStoreContainingPurchases(collectorId, purchCollectionsInputModel);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("collection")]
        public async Task<IActionResult> FetchPurchasedGemsInCollectionFromOneStore([FromBody] PurchAllGemsCollectionInputModel purchAllGemsCollectionInputModel)
        {
            _logger.LogDebug("Entered FetchPurchasedGemsInCollectionFromOneStore end point.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _purchasesService.FetchAllPurchasedGemsInOneCollection(collectorId, purchAllGemsCollectionInputModel);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }
    }
}
