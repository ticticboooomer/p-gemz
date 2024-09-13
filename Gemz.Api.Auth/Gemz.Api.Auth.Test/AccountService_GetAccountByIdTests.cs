using FluentAssertions;
using Gemz.Api.Auth.Data.Model;
using Gemz.Api.Auth.Data.Repository.Account;
using Gemz.Api.Auth.Service.Accounts;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gemz.Api.Auth.Test;

public class AccountService_GetAccountByIdTests
{
    private readonly Mock<IAccountRepository> _accountRepo;
    private readonly Mock<ILogger<AccountService>> _logger;
    private readonly AccountService_Setups _setups;
    
    public AccountService_GetAccountByIdTests()
    {
        _setups = new AccountService_Setups();
        _accountRepo = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<AccountService>>();
    }

    [Fact]
    public void GetAccountById_ShouldResultInReturnedAccount_IfGivenValidAccountId()
    {
        var accountId = _setups.SetupAccountId();
        var accountData = _setups.SetupAccountData(accountId);

        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(accountData);

        var accountService = instantiateAccountService();

        var result = accountService.GetAccountById(accountId);
        
        result.Should().NotBeNull();
        result.Result.Error.Should().BeNull();
        result.Result.Data.Should().NotBeNull();
        result.Result.Data.EmailAddress.Should().Be(accountData.EmailAddress);
        result.Result.Data.TwitchUsername.Should().Be(accountData.TwitchUsername);
        result.Result.Data.IsCreator.Should().Be(accountData.IsCreator);
        result.Result.Data.OnboardingStatus.Should().Be(3);
    }

    [Fact]
    public void GetAccountById_ShouldResultInError_IfGivenBlankAccountId()
    {
        var accountId = string.Empty;

        var accountService = instantiateAccountService();

        var result = accountService.GetAccountById(accountId);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000100");
    }

    [Fact]
    public void GetAccountById_ShouldResultInError_IfGivenAccountIdNotInDatabase()
    {
        var accountId = _setups.SetupAccountId();

        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((Account)null);

       var accountService = instantiateAccountService();

        var result = accountService.GetAccountById(accountId);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000101");
    }

    #region SETUP
   
    private AccountService instantiateAccountService()
    {
        return new AccountService(_accountRepo.Object, _logger.Object);
    }
    
    #endregion
}