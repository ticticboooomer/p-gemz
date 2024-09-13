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

public class TwitchAuthServiceTests
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

    public TwitchAuthServiceTests()
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
    
    #region AUTHORIZE_TESTS
    
    [Fact]
    public void Authorize_ShouldResultInCompleteTwitchUri_IfGivenBasicReturnUri()
    {
        var authStateData = _setups.SetUpAuthStateData();
        _authStateRepo.Setup(x => x.CreateAsync(It.IsAny<AuthState>()))
            .ReturnsAsync(authStateData);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Authorize(authStateData.ReturnUri);

        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().StartWith(_config.Object.Value.Twitch.UrlStart);
        redirectUri.Should().Contain("claims={\"userinfo\":{\"email\":null,\"email_verified\":null,\"preferred_username\":null,\"picture\":null}}");
        redirectUri.Should().Contain($"client_id={_config.Object.Value.Twitch.ClientId}");
        redirectUri.Should().Contain($"scope={_config.Object.Value.Twitch.Scope}");
        redirectUri.Should().Contain($"state={authStateData.Id.ToString()}");
        redirectUri.Should().Contain($"redirect_uri={_config.Object.Value.Twitch.RedirectUrl}");
    }

    [Fact]
    public void Authorize_ShouldResultOKWithError_IfGivenBlankReturnUri()
    {
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Authorize(string.Empty);

        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{_config.Object.Value.Twitch.DefaultErrorUrl}?error=AU000001");
    }

    [Fact]
    public void Authorize_ShouldResultInEmptyUri_IfAuthStateCreateFails()
    {
        var authStateData = _setups.SetUpAuthStateData();
        authStateData.Id = string.Empty;
        
        _authStateRepo.Setup(x => x.CreateAsync(It.IsAny<AuthState>()))
            .ReturnsAsync((AuthState)null);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Authorize(authStateData.ReturnUri);

        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000002");
    }
    
    [Fact]
    public void Authorize_ShouldResultInEmptyUri_IfGivenMissingUrlStartAppSetting()
    {
        _config.Object.Value.Twitch.UrlStart = string.Empty;
        
        var authStateData = _setups.SetUpAuthStateData();
        _authStateRepo.Setup(x => x.CreateAsync(It.IsAny<AuthState>()))
            .ReturnsAsync(authStateData);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Authorize(authStateData.ReturnUri);

        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000003");
    }

    [Fact]
    public void Authorize_ShouldResultInEmptyUri_IfGivenMissingClientIdAppSetting()
    {
        _config.Object.Value.Twitch.ClientId = string.Empty;
        
        var authStateData = _setups.SetUpAuthStateData();
        _authStateRepo.Setup(x => x.CreateAsync(It.IsAny<AuthState>()))
            .ReturnsAsync(authStateData);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Authorize(authStateData.ReturnUri);

        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000003");
    }

    [Fact]
    public void Authorize_ShouldResultInEmptyUri_IfGivenMissingScopeAppSetting()
    {
        _config.Object.Value.Twitch.Scope = string.Empty;
        
        var authStateData = _setups.SetUpAuthStateData();
        _authStateRepo.Setup(x => x.CreateAsync(It.IsAny<AuthState>()))
            .ReturnsAsync(authStateData);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Authorize(authStateData.ReturnUri);

        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000003");
    }

    [Fact]
    public void Authorize_ShouldResultInEmptyUri_IfGivenMissingRedirectUrlAppSetting()
    {
        _config.Object.Value.Twitch.RedirectUrl = string.Empty;
        
        var authStateData = _setups.SetUpAuthStateData();
        _authStateRepo.Setup(x => x.CreateAsync(It.IsAny<AuthState>()))
            .ReturnsAsync(authStateData);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Authorize(authStateData.ReturnUri);

        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000003");
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