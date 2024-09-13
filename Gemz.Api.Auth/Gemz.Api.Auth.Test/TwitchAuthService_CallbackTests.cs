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

public class TwitchAuthService_CallbackTests
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
    
    public TwitchAuthService_CallbackTests()
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
    
    #region CALLBACK_TESTS
    
    [Fact]
    public void Callback_ShouldResultInCompleteTwitchAuthorizeResultModel_IfGivenValidAccessCodeAndState()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var accountData = _setups.SetUpAccountData();
        var twitchUserData = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponse = _setups.SetupTwitchUserInfoResponse();

        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);

        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserData);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponse);
        
        _accountRepo.Setup(x => x.GetByTwitchUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(accountData);
        _accountRepo.Setup(x => x.PatchTwitchData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
                                                                            callbackParams.State, 
                                                                            callbackParams.Error, 
                                                                            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().StartWith(authStateData.ReturnUri);
        redirectUri.Should().Contain("?token=");
        var redirectUrlElements = redirectUri.Split("=");
        redirectUrlElements[1].Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void Callback_ShouldResultInDefaultErrorResponse_IfGivenBlankState()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), string.Empty, string.Empty, string.Empty);
    
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{_config.Object.Value.Twitch.DefaultErrorUrl}?error=AU000004");
    }
    
    [Fact]
    public void Callback_ShouldResultInDefaultErrorResponse_IfGivenStateNotInAuthStateTable()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(new AuthState());
    
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{_config.Object.Value.Twitch.DefaultErrorUrl}?error=AU000005");
    }


    [Fact]
    public void Callback_ShouldResultInDefaultErrorResponse_IfGivenBlankAccessCode()
    {
        var callbackParams =
            _setups.SetUpCallbackParams(string.Empty, Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();

        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);

        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode,
            callbackParams.State,
            callbackParams.Error,
            callbackParams.ErrorDescription);

        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000006");
    }

    [Fact]
    public void Callback_ShouldResultInReturnUriWithTwitchErrorQuerystring_IfGivenTwitchErrorResponse()
    {
        var authStateData = _setups.SetUpAuthStateData();
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        var twitchError = "IAmATwitchError";
        var twitchErrorDescription = "IAmATwitchErrorDescription";
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), twitchError, twitchErrorDescription);
    
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().StartWith($"{authStateData.ReturnUri}?error=AU000007");
        redirectUri.Should().Contain($"&twerror={twitchError}");
        redirectUri.Should().Contain($"&twdescription={twitchErrorDescription}");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfGivenMissingConfigTokenEndpoint()
    {
        _config.Object.Value.Twitch.TokenEndpoint = string.Empty;
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataResponseErr = _setups.SetupTwitchUserDataResponseError("AU000008");
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);

        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponseErr);

        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000008");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfGivenMissingConfigClientId()
    {
        _config.Object.Value.Twitch.ClientId = string.Empty;
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataError = _setups.SetupTwitchUserDataResponseError("AU000008");
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataError);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000008");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfGivenMissingConfigClientSecret()
    {
        _config.Object.Value.Twitch.ClientSecret = string.Empty;
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataError = _setups.SetupTwitchUserDataResponseError("AU000008");
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataError);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000008");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfGivenMissingConfigRedirectUrl()
    {
        _config.Object.Value.Twitch.RedirectUrl = string.Empty;
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataError = _setups.SetupTwitchUserDataResponseError("AU000008");
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataError);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000008");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfGivenBadRequestFromTwitch()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserResponseError = _setups.SetupTwitchUserDataResponseError("AU000009");
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);

        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserResponseError);
    
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000009");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfGivenConfigUserInfoEndpointMissing()
    {
        _config.Object.Value.Twitch.UserInfoEndpoint = string.Empty;
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataResponse = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponseErr = _setups.SetupTwitchUserInfoResponseError("AU000010");
        
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);

        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponse);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponseErr);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000010");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfTwitchReturnsBadRequest()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataResponse = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponseErr = _setups.SetupTwitchUserInfoResponseError("AU000011");
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponse);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponseErr);

        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000011");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfTwitchReturnsUserInfoBlankId()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataResponse = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponse = _setups.SetupTwitchUserInfoResponse();
        twitchUserInfoResponse.TwitchId = string.Empty;
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponse);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponse);

        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000018");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfTwitchReturnsUserInfoBlankEmail()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataResponse = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponse = _setups.SetupTwitchUserInfoResponse();
        twitchUserInfoResponse.Email = string.Empty;
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponse);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponse);

        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000018");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfTwitchReturnsUsernameBlank()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataResponse = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponse = _setups.SetupTwitchUserInfoResponse();
        twitchUserInfoResponse.PreferredUsername = string.Empty;
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponse);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponse);

        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000018");
    }

    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfUpdateExistingAccountFails()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var accountData = _setups.SetUpAccountData();
        var twitchUserDataResponse = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponse = _setups.SetupTwitchUserInfoResponse();
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponse);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponse);

         _accountRepo.Setup(x => x.GetByTwitchUserIdAsync(It.IsAny<string>()))
                        .ReturnsAsync(accountData);
         _accountRepo.Setup(x => x.PatchTwitchData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                        .ReturnsAsync(false);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000012");
    }
    
    [Fact]
    public void Callback_ShouldResultReturnUriWithErrorQuerystring_IfCreateNewAccountFails()
    {
        var callbackParams = _setups.SetUpCallbackParams(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), string.Empty, string.Empty);
        var authStateData = _setups.SetUpAuthStateData();
        var twitchUserDataResponse = _setups.SetupTwitchUserDataResponse();
        var twitchUserInfoResponse = _setups.SetupTwitchUserInfoResponse();
    
        _authStateRepo.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
        _authStateRepo.Setup(x => x.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(authStateData);
    
        _getTwitchAccessToken.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserDataResponse);

        _getUserInfoFromTwitch.Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(twitchUserInfoResponse);

        _accountRepo.Setup(x => x.GetByTwitchUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Account)null);
        _accountRepo.Setup(x => x.CreateAsync(It.IsAny<Account>()))
            .ReturnsAsync((Account)null);
        
        var twitchAuthService = InstantiateTwitchAuthService();
        var result = twitchAuthService.Callback(callbackParams.AccessCode, 
            callbackParams.State, 
            callbackParams.Error, 
            callbackParams.ErrorDescription);
        
        var redirectUri = result.Result.RedirectUri;
        redirectUri.Should().NotBeNullOrEmpty();
        redirectUri.Should().Be($"{authStateData.ReturnUri}?error=AU000012");
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