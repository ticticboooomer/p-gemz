using Gemz.Api.Creator.Middleware;
using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers
{
    [ApiController]
    [Route("api/interest")]
    public class InterestController : ControllerBase
    {
        private readonly IInterestService _interestService;
        private readonly ILogger<InterestController> _logger;

        public InterestController(ILogger<InterestController> logger, IInterestService interestService)
        {
            _logger = logger;
            _interestService = interestService;
        }

        [AllowNonCreator]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterInterestForAccess([FromBody] RegisterInterestInputModel registerInterestInputModel)
        {
            _logger.LogDebug("Entered RegisterInterestForAccess end point on Creator API.");

            var accountId = HttpContext.Items["Account"].ToString();

            var res = await _interestService.RegisterInterest(accountId, registerInterestInputModel);

            _logger.LogDebug("Returned from Service function.");
            return Ok(res);
        }

    }
}
