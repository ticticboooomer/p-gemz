using FluentAssertions;
using Gemz.Api.Auth.Data.Model;
using Gemz.Api.Auth.Data.Repository.Account;
using Gemz.Api.Auth.Data.Repository.AuthState;
using Gemz.Api.Auth.Service.Auth;
using Gemz.Api.Auth.Service.Auth.HttpServices;
using Gemz.Api.Auth.Service.Auth.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Gemz.Api.Auth.Test;

public class TwitchAuthService_RefreshTests
{
    private readonly Mock<IAuthStateRepository> _authStateRepo;
    private readonly Mock<IOptions<AuthConfig>> _config;
    private readonly Mock<IAccountRepository> _accountRepo;
    private readonly Mock<ILogger<TwitchAuthService>> _logger;
    private readonly Mock<IGetTwitchAccessToken> _getTwitchAccessToken;
    private readonly Mock<IGetUserInfoFromTwitch> _getUserInfoFromTwitch;
    private readonly Mock<IRefreshTwitchToken> _refreshTwitchToken;
    private readonly Mock<IValidateTwitchAccessToken> _validateTwitchAccessToken;
    private readonly TwitchAuthService_Setups _setups;
    
    public TwitchAuthService_RefreshTests()
    {
        _setups = new TwitchAuthService_Setups();
        _config = _setups.SetUpAuthConfig();
        _authStateRepo = new Mock<IAuthStateRepository>();
        _accountRepo = new Mock<IAccountRepository>();
        _logger = new Mock<ILogger<TwitchAuthService>>();
        _getTwitchAccessToken = new Mock<IGetTwitchAccessToken>();
        _getUserInfoFromTwitch = new Mock<IGetUserInfoFromTwitch>();
        _refreshTwitchToken = new Mock<IRefreshTwitchToken>();
        _validateTwitchAccessToken = new Mock<IValidateTwitchAccessToken>();
    }
    
    #region REFRESH_TESTS
    
    [Fact]
    public void Refresh_ShouldResultInUpdatedJwt_IfGivenValidJwt()
    {
        var account = _setups.SetUpAccountData();
        var currentJwt = _setups.SetupTestJwt(account.Id, _config.Object.Value);
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(account);        
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Refresh(currentJwt);

        var error = result.Result.Error;
        error.Should().BeNullOrEmpty();
        
        var newJwt = result.Result.Data.AccessCode;
        newJwt.Should().NotBeNullOrEmpty();
        var accountFromJwt = _setups.CheckValidJwt(newJwt, _config.Object.Value);
        accountFromJwt.Should().NotBeNullOrEmpty();
        accountFromJwt[0].Should().Be(account.Id);
        accountFromJwt[1].ToLower().Should().Be("false");
        accountFromJwt[2].Should().Be("0");
    }

    [Fact]
    public void Refresh_ShouldResultInUpdatedJwt_IfGivenValidJwtAndAccountIsCreatorChanged()
    {
        var account = _setups.SetUpAccountData();
        var currentJwt = _setups.SetupTestJwt(account.Id, _config.Object.Value);
        account.IsCreator = true;
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(account);        
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Refresh(currentJwt);

        var error = result.Result.Error;
        error.Should().BeNullOrEmpty();
        
        var newJwt = result.Result.Data.AccessCode;
        newJwt.Should().NotBeNullOrEmpty();
        var accountFromJwt = _setups.CheckValidJwt(newJwt, _config.Object.Value);
        accountFromJwt.Should().NotBeNullOrEmpty();
        accountFromJwt[0].Should().Be(account.Id);
        accountFromJwt[1].ToLower().Should().Be("true");
        accountFromJwt[2].Should().Be("0");
    }

    [Fact]
    public void Refresh_ShouldResultInError_IfGivenValidJwtButAccountNotExist()
    {
        var account = _setups.SetUpAccountData();
        var currentJwt = _setups.SetupTestJwt(account.Id, _config.Object.Value);
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((Account)null);        
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Refresh(currentJwt);

        var error = result.Result.Error;
        error.Should().NotBeNullOrEmpty();
        error.Should().Be("AU000018");

        var newJwt = result.Result.Data?.AccessCode;
        newJwt.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Refresh_ShouldResultInEmptyJwt_IfGivenEmptyJwt()
    {
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Refresh(string.Empty);
        
        var error = result.Result.Error;
        error.Should().NotBeNullOrEmpty();
        error.Should().Be("AU000013");

        var newJwt = result.Result.Data?.AccessCode;
        newJwt.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Refresh_ShouldResultInEmptyJwt_IfGivenInvalidJwt()
    {
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Refresh("IAmARubbishJwtAndIShouldFailValidation");
        
        
        
        var error = result.Result.Error;
        error.Should().NotBeNullOrEmpty();
        error.Should().Be("AU000014");

        var newJwt = result.Result.Data?.AccessCode;
        newJwt.Should().BeNullOrEmpty();
    }

    #endregion
    
    #region SETUPS

    private TwitchAuthService InstantiateTwitchAuthService()
    {
        return new TwitchAuthService(
            _authStateRepo.Object, 
            _config.Object, 
            _accountRepo.Object, 
            _getTwitchAccessToken.Object, 
            _getUserInfoFromTwitch.Object, 
            _validateTwitchAccessToken.Object, 
            _refreshTwitchToken.Object, 
            _logger.Object);
    }

    #endregion
}