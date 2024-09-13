using Gemz.Api.Creator.Service.Creator;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly ILogger<StripeController> _logger;

        public StripeController(IStripeService stripeService, ILogger<StripeController> logger)
        {
            _stripeService = stripeService;
            _logger = logger;
        }

        [HttpPost("stripestatus")]
        public async Task<IActionResult> CheckStripeOnboardingStatus()
        {
            _logger.LogDebug("Entered CheckStripeOnboardingStatus end point on Stripe Endpoint.");

            var creatorId = HttpContext.Items["Account"]?.ToString();
            if (creatorId is null)
            {
                return BadRequest();
            }

            var res = await _stripeService.CheckStripeOnboardingStatus(creatorId);
            _logger.LogDebug($"Returned from Service function.");
            return Ok(res);
        }

        [HttpPost("onboardinglink")]
        public async Task<IActionResult> CreateAccountLink()
        {
            _logger.LogDebug("Entered CreateAccountLink end point on Stripe Endpoint.");

            var creatorId = HttpContext.Items["Account"]?.ToString();
            if (creatorId is null)
            {
                return BadRequest();
            }

            var res = await _stripeService.CreateOnboardingAccountLink(creatorId);
            _logger.LogDebug($"Returned from Service function.");
            return Ok(res);
        }
    }
}
