using Gemz.Api.Collector.Service.Collector.Model;
using Gemz.Api.Collector.Service.Collector;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace Gemz.Api.Collector.Controllers
{
    [ApiController]
    [Route("api/collectorpacks")]
    public class CollectorPacksController : ControllerBase
    {
        private readonly ICollectorPackService _collectorPackService;
        private readonly ILogger<CollectorPacksController> _logger;

        public CollectorPacksController(ICollectorPackService collectorPackService, ILogger<CollectorPacksController> logger)
        {
            _collectorPackService = collectorPackService;
            _logger = logger;
        }

        [HttpPost("unopened")]
        public async Task<IActionResult> FetchUnopenedPacksForCollector()
        {
            _logger.LogDebug("Entered FetchUnopenedPacksForCollector end point.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _collectorPackService.FetchUnopenedPacksForCollector(collectorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("open")]
        public async Task<IActionResult> OpenCollectorPacksInCollection([FromBody] OpenPacksInputModel openPacksInputModel)
        {
            _logger.LogDebug("Entered OpenCollectorPacksInCollection end point.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _collectorPackService.OpenCollectorPacksInCollection(collectorId, openPacksInputModel);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("checksession")]
        public async Task<IActionResult> CheckOpenPacksSession([FromBody] OpenPacksSessionInputModel openPacksSessionInputModel)
        {
            _logger.LogDebug("Entered CheckOpenPacksSession end point.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _collectorPackService.CheckOpenPackSession(collectorId, openPacksSessionInputModel);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }
    }
}
