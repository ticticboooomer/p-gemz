using Gemz.Api.Collector.Middleware;
using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers
{ 
    [ApiController]
    [Route("api/gems")]
    public class GemController : ControllerBase
    {
        private readonly IGemService _gemService;
        private readonly ILogger<GemController> _logger;

        public GemController(IGemService gemService, ILogger<GemController> logger)
        {
            _gemService = gemService;
            _logger = logger;
        }

        [AllowUnauthorized]
        [HttpPost("qtygems")]
        public async Task<IActionResult> GetFixedQtyGemsForCollection([FromBody] GemFixedQtyInputModel gemFixedQtyInputModel)
        {
            _logger.LogDebug("Entered GetFixedQtyGemsForCollection end point.");

            var res = await _gemService.FetchFixedQuantityOfGems(gemFixedQtyInputModel);

            _logger.LogDebug($"Returned from Service function.");

            return Ok(res);
        }

        [AllowUnauthorized]
        [HttpPost("page")]
        public async Task<IActionResult> GetPageOfGemsForCollection([FromBody] GemPagingModel gemPagingModel)
        {
            _logger.LogDebug("Entered GetPageOfGemsForCollection end point.");

            var res = await _gemService.GetPagedGemsForCollection(gemPagingModel);

            _logger.LogDebug($"Returned from Service function.");

            return Ok(res);
        }

        [AllowUnauthorized]
        [HttpPost("gem")]
        public async Task<IActionResult> GetSingleGem([FromBody] SingleGemInputModel singleGemInputModel)
        {
            _logger.LogDebug("Entered GetPageOfGemsForCollection end point.");

            var res = await _gemService.GetSingleGemById(singleGemInputModel);

            _logger.LogDebug($"Returned from Service function.");

            return Ok(res);
        }

    }
}
