using Gemz.Api.Auth.Middleware;
using Gemz.Api.Auth.Service.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ITwitchAuthService _twitchAuth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITwitchAuthService twitchAuth, ILogger<AuthController> logger)
    {
        _twitchAuth = twitchAuth;
        _logger = logger;
    }
    
    [AllowUnauthorized]
    [HttpGet("twitch")]
    public async Task<IActionResult> Twitch(string returnUri)
    {
        _logger.LogDebug("Entered /twitch end point");
        _logger.LogInformation($"Parameter returnUri: {returnUri}");
        
        var res = await _twitchAuth.Authorize(returnUri);
        
        _logger.LogDebug($"Service Call to Authorise returned");
        _logger.LogInformation($"Returned redirect Uri: {res.RedirectUri}");
        return Redirect(res.RedirectUri);
    }

    [AllowUnauthorized]
    [HttpGet("twitch/callback")]
    public async Task<IActionResult> TwitchCallback(string code, string state,string error="", string errorDescription="")
    {
        _logger.LogDebug("Entered /twitch/callback end point");
        _logger.LogInformation($"Parameters: code: (private data) | state: {state} | error: {error} | errorDescription: {errorDescription}");
        
        var res = await _twitchAuth.Callback(code, state, error, errorDescription);

        _logger.LogDebug($"Service Call to Callback returned");
        
        _logger.LogInformation($"Returned redirect Uri: {res.RedirectUri}");
        
        return Redirect(res.RedirectUri);
    }
    
    [AllowUnauthorized]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(string token)
    {
        _logger.LogDebug("Entered /refresh end point");
        _logger.LogInformation($"Parameter: token: {token}");
        
        var res = await _twitchAuth.Refresh(token);

        _logger.LogDebug($"Service Call to Refresh returned");
        
        return Ok(res);
    }

    [AllowUnauthorized]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate(string token)
    {
        _logger.LogDebug("Entered /validate end point");
        _logger.LogInformation($"Parameter: token: {token}");
        
        var res = await _twitchAuth.Validate(token);

        _logger.LogDebug("Service Call to Validate returned");

        if (!res.Data.IsValid)
        {
            return new UnauthorizedResult();
        }
        
        return Ok(res);
    }
}