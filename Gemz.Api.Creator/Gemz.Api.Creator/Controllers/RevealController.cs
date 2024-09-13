using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers
{
    [ApiController]
    [Route("api/reveal")]
    public class RevealController : ControllerBase
    {
            private readonly IRevealService _revealService;
            private readonly ILogger<RevealController> _logger;

            public RevealController(IRevealService revealService, ILogger<RevealController> logger)
            {
                _revealService = revealService;
                _logger = logger;
            }

            [HttpPost("list")]
            public async Task<IActionResult> ListOfGemsToBeRevealed()
            {
                _logger.LogDebug("Entered ListOfGemsToBeRevealed end point on Creator API.");
                if (HttpContext?.Items != null)
                {
                    var accountParam = HttpContext.Items["Account"];
                    if (accountParam != null)
                    {
                        var creatorId = accountParam.ToString();
                        if (creatorId != null)
                        {
                            var res = await _revealService.GetGemsTobeRevealedByCreator(creatorId);
                            _logger.LogDebug($"Returned from Service function.");
                            return Ok(res);
                        }
                    }
                }
                return BadRequest();
            }

        [HttpPost("singlegem")]
        public async Task<IActionResult> RevealSinlgeGem([FromBody] SingleGemRevealInputModel singleGemRevealInputModel)
        {
            _logger.LogDebug("Entered RevealSinlgeGem end point on Creator API.");
            if (HttpContext?.Items != null)
            {
                var accountParam = HttpContext.Items["Account"];
                if (accountParam != null)
                {
                    var creatorId = accountParam.ToString();
                    if (creatorId != null)
                    {
                        var res = await _revealService.RevealSingleGem(creatorId, singleGemRevealInputModel);
                        _logger.LogDebug($"Returned from Service function.");
                        return Ok(res);
                    }
                }
            }
            return BadRequest();
        }

    }
}
