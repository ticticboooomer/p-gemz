using Gemz.Api.Collector.Service.Collector;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService,ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;

        }


        [HttpPost("totalgemspurchased")]
        public async Task<IActionResult> GetTotalGemsPurchased()
        {
            _logger.LogDebug("Entered GetTotalGemsPurchased end point on CollectorApi.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _dashboardService.GetTotalGemsPurchased(collectorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("totalgemsopened")]
        public async Task<IActionResult> GetTotalGemsOpened()
        {
            _logger.LogDebug("Entered GetTotalGemsOpened end point on CollectorApi.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _dashboardService.GetTotalGemsOpened(collectorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("totalgemsunopened")]
        public async Task<IActionResult> GetTotalGemsUnopened()
        {
            _logger.LogDebug("Entered GetTotalGemsUnopened end point on CollectorApi.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _dashboardService.GetTotalGemsUnopened(collectorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

        [HttpPost("totalsbycreator")]
        public async Task<IActionResult> GetTotalGemsByCreator()
        {
            _logger.LogDebug("Entered GetTotalGemsByCreator end point on CollectorApi.");

            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var collectorId = accountParam.ToString();
                    if (collectorId != null)
                    {
                        var res = await _dashboardService.GetTotalGemsByCreator(collectorId);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

    }
}

