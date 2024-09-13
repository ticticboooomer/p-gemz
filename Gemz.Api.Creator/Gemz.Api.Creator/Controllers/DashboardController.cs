using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpPost("collectionstats")]
        public async Task<IActionResult> CollectionStats()
        {
            _logger.LogDebug("Entered CollectionStats end point on Creator API.");
            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var creatorId = accountParam.ToString();
                    if (creatorId != null)
                    {
                        var res = await _dashboardService.GetCollectionStats(creatorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("gemstoopenedstats")]
        public async Task<IActionResult> GemsTobeOpened()
        {
            _logger.LogDebug("Entered GemsTobeOpened end point on Creator API.");
            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var creatorId = accountParam.ToString();
                    if (creatorId != null)
                    {
                        var res = await _dashboardService.GetGemsToBeOpenedStats(creatorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }
    }
}
