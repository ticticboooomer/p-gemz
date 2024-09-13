using FluentAssertions;
using Gemz.Api.Auth.Data.Model;
using Gemz.Api.Auth.Data.Repository.Account;
using Gemz.Api.Auth.Service.Accounts;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gemz.Api.Auth.Test;

public class AccountService_UpdateAccountTests
{
    private readonly Mock<IAccountRepository> _accountRepo;
    private readonly Mock<ILogger<AccountService>> _logger;
    private readonly AccountService_Setups _setups;
    
    public AccountService_UpdateAccountTests()
    {
        _setups = new AccountService_Setups();
        _accountRepo = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<AccountService>>();
    }

    [Fact]
    public void UpdateAccount_ShouldResultInReturnedAccount_IfGivenValidData()
    {
        var accountId = _setups.SetupAccountId();
        var accountData = _setups.SetupAccountData(accountId);
        var accountUpdateModel = _setups.SetupAccountUpdateModel();
        var returnedAccountModel = _setups.SetupAccountModel(accountUpdateModel.EmailAddress, accountData);
        
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(accountData);

        _accountRepo.Setup(x => x.PatchEmailAddress(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        var accountService = instantiateAccountService();

        var result = accountService.UpdateAccount(accountId, accountUpdateModel);
        
        result.Should().NotBeNull();
        result.Result.Error.Should().BeNull();
        result.Result.Data.Should().NotBeNull();
        result.Result.Data.EmailAddress.Should().Be(returnedAccountModel.EmailAddress);
        result.Result.Data.TwitchUsername.Should().Be(returnedAccountModel.TwitchUsername);
        result.Result.Data.IsCreator.Should().Be(returnedAccountModel.IsCreator);
        result.Result.Data.OnboardingStatus.Should().Be(3);
    }

    [Fact]
    public void UpdateAccount_ShouldResultInError_IfGivenBlankAccountId()
    {
        var accountId = string.Empty;
        var accountUpdateModel = _setups.SetupAccountUpdateModel();
        
        var accountService = instantiateAccountService();

        var result = accountService.UpdateAccount(accountId, accountUpdateModel);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000200");
    }

    [Fact]
    public void UpdateAccount_ShouldResultInError_IfGivenBlankEmailAddress()
    {
        var accountId = _setups.SetupAccountId();
        var accountUpdateModel = _setups.SetupAccountUpdateModel();
        accountUpdateModel.EmailAddress = string.Empty;
        
        var accountService = instantiateAccountService();

        var result = accountService.UpdateAccount(accountId, accountUpdateModel);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000201");
    }

    [Fact]
    public void UpdateAccount_ShouldResultInError_IfGivenEmailAddressTooLong()
    {
        var accountId = _setups.SetupAccountId();
        var accountUpdateModel = _setups.SetupAccountUpdateModel();
        // Set too long 321 chars
        var tooLargeEmailAddress = string.Empty;
        for (var i = 0; i < 321; i++)
        {
            tooLargeEmailAddress += "a";
        }
        accountUpdateModel.EmailAddress = tooLargeEmailAddress;
        
        var accountService = instantiateAccountService();

        var result = accountService.UpdateAccount(accountId, accountUpdateModel);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000204");
    }

    [Fact]
    public void UpdateAccount_ShouldResultInError_IfGivenEmailAddressWithInvalidFormat()
    {
        var accountId = _setups.SetupAccountId();
        var accountUpdateModel = _setups.SetupAccountUpdateModel();
        accountUpdateModel.EmailAddress = "12345678901234567890";
        
        var accountService = instantiateAccountService();

        var result = accountService.UpdateAccount(accountId, accountUpdateModel);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000205");
    }

    [Fact]
    public void UpdateAccount_ShouldResultInError_IfGivenAccountIdNotInDatabase()
    {
        var accountId = _setups.SetupAccountId();
        var accountUpdateModel = _setups.SetupAccountUpdateModel();
        
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((Account)null);

        var accountService = instantiateAccountService();

        var result = accountService.UpdateAccount(accountId, accountUpdateModel);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000202");
    }

    [Fact]
    public void UpdateAccount_ShouldResultInError_IfValidDataButUpdateFailsInRepo()
    {
        var accountId = _setups.SetupAccountId();
        var accountData = _setups.SetupAccountData(accountId);
        var accountUpdateModel = _setups.SetupAccountUpdateModel();
        
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(accountData);

        _accountRepo.Setup(x => x.PatchEmailAddress(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var accountService = instantiateAccountService();

        var result = accountService.UpdateAccount(accountId, accountUpdateModel);
        
        result.Should().NotBeNull();
        result.Result.Data.Should().BeNull();
        result.Result.Error.Should().NotBeNull();
        result.Result.Error.Should().Be("AS000203");
    }

    #region SETUP
   
    private AccountService instantiateAccountService()
    {
        return new AccountService(_accountRepo.Object, _logger.Object);
    }
    
    #endregion
    
}