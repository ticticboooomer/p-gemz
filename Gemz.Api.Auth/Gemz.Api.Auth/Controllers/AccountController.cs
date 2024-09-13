using Gemz.Api.Auth.Service.Accounts;
using Gemz.Api.Auth.Service.Accounts.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Auth.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost("get")]
    public async Task<IActionResult> GetAccountById()
    {
        _logger.LogDebug("Entered GetAccountById");

        var accountId = HttpContext.Items["Account"].ToString();
        _logger.LogInformation($"AccountId: {accountId}");

        var result = await _accountService.GetAccountById(accountId);

        _logger.LogDebug("Returned from AccountService");

        return Ok(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountUpdateModel accountUpdateModel)

{
        _logger.LogDebug("Entered UpdateAccount");

        var accountId = HttpContext.Items["Account"].ToString();
        _logger.LogInformation($"AccountId: {accountId}");
        
        var result = await _accountService.UpdateAccount(accountId, accountUpdateModel);
        
        _logger.LogDebug("Returned from AccountService");

        return Ok(result);
    }
}