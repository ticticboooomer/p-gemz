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

public class TwitchAuthService_ValidateTests
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
    
    public TwitchAuthService_ValidateTests()
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

#region VALIDATE_TESTS
    
    [Fact]
    public void Validate_ShouldResultInTrue_IfGivenValidJwt()
    {
        var accountData = _setups.SetUpAccountData();
        var currentJwt = _setups.SetupTestJwt(accountData.Id, _config.Object.Value);

        _validateTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(accountData);
    
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Validate(currentJwt);

        var error = result.Result.Error;
        error.Should().BeNullOrEmpty();
        var validStatus = result.Result.Data.IsValid;
        validStatus.Should().Be(true);
    }
    
    [Fact]
    public void Validate_ShouldResultInTrue_IfGivenValidJwtButTwitchTokensNeedRefresh()
    {
        var accountData = _setups.SetUpAccountData();
        var currentJwt = _setups.SetupTestJwt(accountData.Id, _config.Object.Value);
        var twitchAuthCodeResponse = _setups.SetupTwitchAuthCodeResponse();

        _validateTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(false);

        _refreshTwitchToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchAuthCodeResponse);
        
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(accountData);
        _accountRepo.Setup(x => x.PatchTwitchTokens(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Validate(currentJwt);
        
        var error = result.Result.Error;
        error.Should().BeNullOrEmpty();
        result.Result.Data.IsValid.Should().Be(true);
    }
    
    [Fact]
    public void Validate_ShouldResultInFalse_IfGivenEmptyJwt()
    {
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Validate(string.Empty);
        
        var error = result.Result.Error;
        var validStatus = result.Result.Data;
        error.Should().Be("AU000015");
        validStatus.Should().Be(null);
    }
    
    [Fact]
    public void Validate_ShouldResultInFalse_IfGivenInvalidJwt()
    {
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Validate("IAmAnInvalidJwtAndIShouldFailValidation");
        
        var error = result.Result.Error;
        var validStatus = result.Result.Data;
        error.Should().Be("AU000016");
        validStatus.Should().Be(null);
    }
    
    [Fact]
    public void Validate_ShouldResultInFalse_IfGivenValidJwtWithNonExistingAccountId()
    {
        var accountData = _setups.SetUpAccountData();
        var currentJwt = _setups.SetupTestJwt(accountData.Id, _config.Object.Value);
        
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync((Account)null);
    
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Validate(currentJwt);
        
        var error = result.Result.Error;
        var validStatus = result.Result.Data;
        error.Should().Be("AU000017");
        validStatus.Should().Be(null);
    }
    
    [Fact]
    public void Validate_ShouldResultInFalse_IfGivenValidJwtButTwitchTokensNeedRefreshAndFails()
    {
        var accountData = _setups.SetUpAccountData();
        var currentJwt = _setups.SetupTestJwt(accountData.Id, _config.Object.Value);
        var twitchAuthCodeResponseErr = _setups.SetupTwitchAuthCodeResponseError("erroroccurred");

        _validateTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(false);

        _refreshTwitchToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchAuthCodeResponseErr);
        
        _accountRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(accountData);
    
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Validate(currentJwt);
        
        var error = result.Result.Error;
        var validStatus = result.Result.Data.IsValid;
        error.Should().BeNullOrEmpty();
        validStatus.Should().Be(false);
    }
    
#endregion
  
#region SETUP
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