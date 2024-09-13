using System.Text.RegularExpressions;
using Gemz.Api.Auth.Data.Model;
using Gemz.Api.Auth.Data.Repository.Account;
using Gemz.Api.Auth.Service.Accounts.Model;
using Gemz.Api.Auth.Service.Auth.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Auth.Service.Accounts;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepo;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IAccountRepository accountRepo, ILogger<AccountService> logger)
    {
        _accountRepo = accountRepo;
        _logger = logger;
    }

    public async Task<GenericResponse<AccountModel>> GetAccountById(string accountId)
    {
        _logger.LogDebug("Entered GetAccountById");
        _logger.LogInformation($"Account Id: {accountId}");

        if (string.IsNullOrEmpty(accountId))
        {
            _logger.LogWarning("Account Id parameter is empty");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000100"
            };
        }

        var accountData = await _accountRepo.GetAsync(accountId);

        if (accountData is null)
        {
            _logger.LogWarning("Account was not found in Database");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000101"
            };
        }

        return new GenericResponse<AccountModel>()
        {
            Data = await MapAccountToAccountModel(accountData)
        };
    }

    public async Task<GenericResponse<AccountModel>> UpdateAccount(string accountId, AccountUpdateModel accountUpdateModel)
    {
        _logger.LogDebug("Entered UpdateAccount");
        _logger.LogInformation($"Account Id: {accountId}");

        if (string.IsNullOrEmpty(accountId))
        {
            _logger.LogWarning("Account Id parameter is empty");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000200"
            };
        }

        if (string.IsNullOrEmpty(accountUpdateModel.EmailAddress))
        {
            _logger.LogWarning("Email Address is empty. Invalid.");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000201"
            };
        }

        if (accountUpdateModel.EmailAddress.Length > 320)
        {
            _logger.LogWarning("Email Address is more than 320 chars. Invalid.");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000204"
            };
        }

        const string emailRegEx = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        var matchEmail = Regex.Match(accountUpdateModel.EmailAddress, emailRegEx, RegexOptions.IgnoreCase);
        if (!matchEmail.Success)
        {
            _logger.LogWarning("Email Address is in an invalid format. Rejected.");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000205"
            };
        }
            
        var accountData = await _accountRepo.GetAsync(accountId);

        if (accountData is null)
        {
            _logger.LogWarning("Account was not found in Database");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000202"
            };
        }

        var updateSuccessful = await _accountRepo.PatchEmailAddress(accountId, accountUpdateModel.EmailAddress);
        if (!updateSuccessful)
        {
            _logger.LogWarning("Problem during PatchEmailAddress operation of Account in database.");
            return new GenericResponse<AccountModel>()
            {
                Error = "AS000203"
            };
        }

        accountData.EmailAddress = accountUpdateModel.EmailAddress;

        return new GenericResponse<AccountModel>()
        {
            Data = await MapAccountToAccountModel(accountData)
        };
    }
    
    private async Task<AccountModel> MapAccountToAccountModel(Account account)
    {
        return new AccountModel()
        {
            EmailAddress = account.EmailAddress,
            TwitchUsername = account.TwitchUsername,
            IsCreator = account.IsCreator,
            OnboardingStatus = (int)account.OnboardingStatus
        };
    }
}