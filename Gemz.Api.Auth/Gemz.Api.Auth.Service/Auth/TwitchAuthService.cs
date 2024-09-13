using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using System.Text.Json;
using Gemz.Api.Auth.Data.Model;
using Gemz.Api.Auth.Data.Repository.Account;
using Gemz.Api.Auth.Data.Repository.AuthState;
using Gemz.Api.Auth.Service.Auth.HttpServices;
using Gemz.Api.Auth.Service.Auth.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Gemz.Api.Auth.Service.Accounts.Model;

namespace Gemz.Api.Auth.Service.Auth;

public class TwitchAuthService : ITwitchAuthService
{
    private readonly IAuthStateRepository _authStateRepo;
    private readonly IOptions<AuthConfig> _config;
    private readonly IAccountRepository _accountRepo;
    private readonly IGetTwitchAccessToken _getTwitchAccessToken;
    private readonly IGetUserInfoFromTwitch _getUserInfoFromTwitch;
    private readonly IValidateTwitchAccessToken _validateTwitchAccessToken;
    private readonly IRefreshTwitchToken _refreshTwitchToken;
    private readonly ILogger<TwitchAuthService> _logger;

    public TwitchAuthService(IAuthStateRepository authStateRepo, 
        IOptions<AuthConfig> config, 
        IAccountRepository accountRepo,
        IGetTwitchAccessToken getTwitchAccessToken,
        IGetUserInfoFromTwitch getUserInfoFromTwitch,
        IValidateTwitchAccessToken validateTwitchAccessToken,
        IRefreshTwitchToken refreshTwitchToken,
        ILogger<TwitchAuthService> logger)
    {
        _authStateRepo = authStateRepo;
        _config = config;
        _accountRepo = accountRepo;
        _getTwitchAccessToken = getTwitchAccessToken;
        _getUserInfoFromTwitch = getUserInfoFromTwitch;
        _validateTwitchAccessToken = validateTwitchAccessToken;
        _refreshTwitchToken = refreshTwitchToken;
        _logger = logger;
    }
    
    public async Task<TwitchAuthorizeResultModel> Authorize(string returnUri)
    {
        _logger.LogDebug("Entered Authorise function");
        var twitchAuthorizeResultModel = new TwitchAuthorizeResultModel();

        if (string.IsNullOrEmpty(returnUri))
        {
            _logger.LogWarning("Return Uri missing. Leaving Function.");
            twitchAuthorizeResultModel.RedirectUri = await DefaultErrorResponse();
            twitchAuthorizeResultModel.RedirectUri += "?error=AU000001";
            return twitchAuthorizeResultModel;
        }
        
        _logger.LogInformation($"Parameter returnUri: {returnUri}");

        var state = await SaveTwitchStateAndReturnUri(returnUri);
        _logger.LogDebug("Returned from SaveTwitchStateAndReturnUri function");

        if (string.IsNullOrEmpty(state))
        {
            _logger.LogWarning("Returned null/empty from SaveTwitchStateAndReturnUri call. Returning empty TwitchAuthorizeResultModel. Leaving Function.");
            twitchAuthorizeResultModel.RedirectUri = $"{returnUri}?error=AU000002";
            return twitchAuthorizeResultModel;
        }
        
        _logger.LogDebug("Calling BuildTwitchAuthoriseUri function");
        var resultUri = await BuildTwitchAuthoriseUri(state);

        if (string.IsNullOrEmpty(resultUri))
        {
            _logger.LogWarning("Returned null/empty resultsUri from BuildTwitchAuthoriseUri call. Returning empty TwitchAuthorizeResultModel. Leaving Function.");
            twitchAuthorizeResultModel.RedirectUri = $"{returnUri}?error=AU000003";
            return twitchAuthorizeResultModel;
        }
        
        _logger.LogDebug("Returned built resultUri to pass back");
        _logger.LogInformation($"resultUri: {resultUri}");

        twitchAuthorizeResultModel.RedirectUri = resultUri;
        return twitchAuthorizeResultModel;
    }

    public async Task<TwitchAuthorizeResultModel> Callback(string accessCode, string state, string error, string errorDescription)
    {
        _logger.LogDebug("Entered Callback function");
        _logger.LogInformation($"Parameters: accessCode: (private data) | state: {state} | error: {error} | errorDescription: {errorDescription}");

        var twitchAuthorizeResultModel = new TwitchAuthorizeResultModel();
        
        if (string.IsNullOrEmpty(state))
        {
               _logger.LogWarning("State parameter missing. Redirecting to default error Uri");
               twitchAuthorizeResultModel.RedirectUri = await DefaultErrorResponse();
               twitchAuthorizeResultModel.RedirectUri += "?error=AU000004";
               return twitchAuthorizeResultModel;
        }
        
        var returnUri = await ValidateReturnedState(state);
        _logger.LogDebug("Returned from ValidateReturnedState function");
        if (string.IsNullOrEmpty(returnUri))
        {
            _logger.LogWarning("returnUri was returned as null or empty. leaving function.");
            twitchAuthorizeResultModel.RedirectUri = await DefaultErrorResponse();
            twitchAuthorizeResultModel.RedirectUri += "?error=AU000005";
            return twitchAuthorizeResultModel;
        }

        twitchAuthorizeResultModel.RedirectUri = returnUri;
        
        if (string.IsNullOrEmpty(accessCode))
        {
            _logger.LogWarning("Empty accessCode passed in to Callback. Leaving function");
            twitchAuthorizeResultModel.RedirectUri += "?error=AU000006";
            return twitchAuthorizeResultModel;
        }
        
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Twitch sent back an error. Leaving function.");
            twitchAuthorizeResultModel.RedirectUri +=
                $"?error=AU000007&twerror={error}&twdescription={errorDescription}";
            return twitchAuthorizeResultModel;
        }

        var twitchUserData = await _getTwitchAccessToken.Execute(accessCode).ConfigureAwait(false);
        
        _logger.LogDebug("Returned from _getTwitchAccessToken");
        if (!string.IsNullOrEmpty(twitchUserData.Error))
        {
            _logger.LogWarning($"UserAccCode or UserRefreshCode were returned as null or empty. Leaving function.");
            twitchAuthorizeResultModel.RedirectUri += $"?error={twitchUserData.Error}";
            return twitchAuthorizeResultModel;
        }

        twitchUserData = await GetUserInfoFromTwitch(twitchUserData);
        
        _logger.LogDebug("Returned from GetUserInfoFromTwitch function");
        if (!string.IsNullOrEmpty(twitchUserData.Error))
        {
            _logger.LogWarning($"Error encountered {twitchUserData.Error} when fetching UserInfo from Twitch. Leaving function.");
            twitchAuthorizeResultModel.RedirectUri += $"?error={twitchUserData.Error}";
            return twitchAuthorizeResultModel;
        }
        
        var account = await UpdateOrCreateAccountData(twitchUserData);
        _logger.LogDebug("Returned from UpdateOrCreateAccountData function");
        if (account == null)
        {
            _logger.LogWarning($"Account object returned as null. Leaving function.");
            twitchAuthorizeResultModel.RedirectUri += "?error=AU000012";
            return twitchAuthorizeResultModel;
        }

        var jwtClaims = new JwtClaimsModel()
        {
            AccountId = account.Id,
            PictureUri = twitchUserData.PictureUri,
            IsCreator = account.IsCreator.ToString(),
            OnboardingStatus = account.OnboardingStatus,
            RestrictedStatus = account.RestrictedStatus
        };
        var jwtToken = await BuildJsonWebToken(jwtClaims);
        _logger.LogDebug("Returned from BuildJsonWebToken function");
        _logger.LogInformation($"Returned jwtToken: {jwtToken}");

        twitchAuthorizeResultModel.RedirectUri += $"?token={jwtToken}";
        return twitchAuthorizeResultModel;
    }

    public async Task<GenericResponse<JwtResultModel>> Refresh(string token)
    {
        _logger.LogDebug("Entered Refresh function");
        _logger.LogInformation($"Parameter: token: {token}");

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("token parameter passed in is null or empty. Leaving Function");
            return new GenericResponse<JwtResultModel>()
            {
                Error = "AU000013"
            };
        }
        
        var jwtClaims = await ValidateToken(token);
        _logger.LogDebug("Returned from ValidateToken function");

        if (string.IsNullOrEmpty(jwtClaims?.AccountId))
        {
            _logger.LogWarning("accountId was returned as null or empty. Leaving function. Returning empty JwtResultModel.");
            return new GenericResponse<JwtResultModel>()
            {
                Error = "AU000014"
            };
        }

        var creatorData = await FetchCreatorDataFromAccount(jwtClaims.AccountId);
        if (!creatorData.ValidData)
        {
            _logger.LogWarning("accountId did not match an account record. Leaving function.");
            return new GenericResponse<JwtResultModel>()
            {
                Error = "AU000018"
            };
        }

        jwtClaims.IsCreator = creatorData.IsCreator ? "True" : "False";
        jwtClaims.OnboardingStatus = creatorData.OnboardingStatus;
        jwtClaims.RestrictedStatus = creatorData.RestrictedStatus;

        _logger.LogInformation($"Returned accountId: {jwtClaims.AccountId} | PictureUri: {jwtClaims.PictureUri}");
        var newJwt = await BuildJsonWebToken(jwtClaims);
        
        _logger.LogDebug("Returned from BuildJsonWebToken function");
        _logger.LogInformation($"Returned newJwt: {newJwt}");

        var jwtResultModel = new JwtResultModel()
        {
            AccessCode = newJwt
        };
        return new GenericResponse<JwtResultModel>()
        {
            Data = jwtResultModel
        };
    }

    public async Task<GenericResponse<ValidateResultModel>> Validate(string token)
    {
        _logger.LogDebug("Entered Validate function");
        _logger.LogInformation($"Parameter: token: {token}");

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("token parameter passed in is null or empty. Leaving Function");
            return new GenericResponse<ValidateResultModel>()
            {
                Error = "AU000015"
            };
        }
        
        var jwtClaims = await ValidateToken(token);

        _logger.LogDebug("Returned from ValidateToken function");
        if (string.IsNullOrEmpty(jwtClaims?.AccountId))
        {
            _logger.LogWarning("accountId was returned as null or empty. Leaving function. Returning False.");
            return new GenericResponse<ValidateResultModel>()
            {
                Error = "AU000016"
            };
        }
        
        _logger.LogInformation($"accountId returned: {jwtClaims} | pictureUri: {jwtClaims.PictureUri}");
        var account = await _accountRepo.GetAsync(jwtClaims.AccountId);
        _logger.LogDebug("Returned from GetAsync function");

        if (account == null)
        {
            _logger.LogWarning("account entity was returned as null. Leaving function. Returning false.");
            return new GenericResponse<ValidateResultModel>()
            {
                Error = "AU000017"
            };
        }

        var validTwitchToken = await _validateTwitchAccessToken.Execute(account.Tokens.AccessCode);
        _logger.LogDebug("Returned from ValidateTwitchAccessToken function");
        
        if (validTwitchToken)
        {
            _logger.LogDebug("Twitch Access Token is still valid. Leaving Function. Returning True.");
            return new GenericResponse<ValidateResultModel>()
            {
                Data = new ValidateResultModel()
                {
                    IsValid = true
                }
            };
        }
        
        _logger.LogDebug("TwitchAccessToken is invalid. Trying Refresh");
        var refreshedTwitchToken = await RefreshTwitchToken(account);
        _logger.LogDebug("Returned from RefreshTwitchToken");

        return new GenericResponse<ValidateResultModel>()
        {
            Data = new ValidateResultModel(){ 
                IsValid = refreshedTwitchToken
            }
        };
    }

    private async Task<string> BuildTwitchAuthoriseUri(string state)
    {
        _logger.LogDebug("Entered BuildTwitchAuthoriseUri function");
        var twitchConfig = _config.Value.Twitch;
        
        _logger.LogDebug("Validating essential Twitch AppSettings");
        if (string.IsNullOrEmpty(twitchConfig.UrlStart)
            || string.IsNullOrEmpty(twitchConfig.ClientId)
            || string.IsNullOrEmpty(twitchConfig.Scope)
            || string.IsNullOrEmpty(twitchConfig.RedirectUrl))
        {
            _logger.LogWarning("One or more Twitch AppSettings are missing. Failed to built Uri. Leaving Function");
            return null;
        }

        var userInfoClaim = new UserInfoClaimsModel()
        {
            UserInfo = new UserInfoClaimsModel.UserInfoClaims()
            {
                Email = null,
                EmailVerified = null,
                TwitchUsername = null,
                PictureUri = null
            }
        };
        var userInfoClaimString = JsonSerializer.Serialize(userInfoClaim);

        _logger.LogDebug("Building and returning Uri");
        return $"{twitchConfig.UrlStart}?" +
               $"claims={userInfoClaimString}" +
               $"&client_id={twitchConfig.ClientId}" +
               $"&scope={twitchConfig.Scope}" +
               $"&response_type=code&state={state}" +
               $"&redirect_uri={twitchConfig.RedirectUrl}";
    }

    private async Task<bool> RefreshTwitchToken(Account account)
    {
        _logger.LogDebug("Entered RefreshTwitchToken function");
        
        var twitchResponse = await _refreshTwitchToken.Execute(account.Tokens.RefreshCode).ConfigureAwait(false);
        _logger.LogDebug("Returned from Twitch call");

        if (!string.IsNullOrEmpty(twitchResponse.ErrorCode))
        {
            _logger.LogWarning("Twitch call returned a BadRequest response. Leaving functioning. Returning false.");
            return false;
        }

        _logger.LogDebug("Converted the twitch returned Json");
        account.Tokens.AccessCode = twitchResponse.AccessToken;
        account.Tokens.RefreshCode = twitchResponse.RefreshToken;

        var patchSucceeded =
            await _accountRepo.PatchTwitchTokens(account.Id, twitchResponse.AccessToken, twitchResponse.RefreshToken);
        _logger.LogInformation($"Returned from UpdateAsync of account with success of: {patchSucceeded}");
        
        return patchSucceeded;
    }
    
    private async Task<JwtClaimsModel> ValidateToken(string token)
    {
        _logger.LogDebug("Entered ValidateToken function");
        _logger.LogInformation($"Parameter token: {token}");
        
        var handler = new JwtSecurityTokenHandler();
        var jwtSettings = _config.Value.Jwt;
        
        try
        {
            _logger.LogDebug("Checking Validity of Token");
            handler.ValidateToken(token, new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            _logger.LogDebug("Passing in JWT token passed validation. Leaving Function. Returning sub claim");
            return new JwtClaimsModel()
            {
                AccountId  = jwtToken.Claims.First(x => x.Type == "sub").Value,
                PictureUri = jwtToken.Claims.First(x => x.Type == "picture").Value,
                IsCreator = jwtToken.Claims.First(x => x.Type == "iscr").Value,
                OnboardingStatus = int.Parse(jwtToken.Claims.First(x => x.Type == "obstatus").Value),
                RestrictedStatus = int.Parse(jwtToken.Claims.First(x => x.Type == "resstatus").Value)
            };
        }
        catch
        {
            _logger.LogError("Passed in JWT token failed validation. Leaving function. Returning null.");
            return null;
        }
    }

    private async Task<string> SaveTwitchStateAndReturnUri(string returnUri)
    {
        _logger.LogDebug("Entered SaveTwitchStateAndReturnUri function");
        var state = Guid.NewGuid().ToString();

        _logger.LogInformation($"Setting new state value: {state}");
        var authState = await _authStateRepo.CreateAsync(new AuthState()
        {
            ReturnUri = returnUri,
            Id = state,
            Provider = "twitch"
        });
        _logger.LogDebug("Returned from CreateAsync on AuthStateRepo function");

        if (authState != null) return authState.Id;
        
        _logger.LogWarning("Problem creating AuthState record. Returning Null.");
        return null;
    }


    private async Task<string> DefaultErrorResponse()
    {
        _logger.LogDebug("Entered  DefaultErrorResponse function");
        var authConfig = _config.Value.Twitch;
        
        _logger.LogInformation($"Returning DefaultErrorUrl: {authConfig.DefaultErrorUrl}");
        return authConfig.DefaultErrorUrl;
    }
    
    private async Task<string> ValidateReturnedState(string state)
    {
        _logger.LogDebug("Entered  ValidateReturnedState function");
        _logger.LogInformation($"Parameter state: {state}");
        
        var authState = await _authStateRepo.GetAsync(state);

        _logger.LogDebug("Returned from GetAsync of AuthStateRepo")
            ;
        var returnUri = authState?.ReturnUri;
        if (!string.IsNullOrEmpty(returnUri))
        {
            _logger.LogInformation($"GetSync of AuthStateRepo returned returnUri: {returnUri}");
            var resp = await _authStateRepo.DeleteAsync(state);
            _logger.LogDebug("Returned from DeleteAsync of AuthStateRepo");
        }
        else
        {
            _logger.LogDebug("No matching AuthState record returned from Database");
        }
        return (string.IsNullOrEmpty(authState?.ReturnUri) ? string.Empty : authState.ReturnUri);
    }

    private async Task<TwitchUserData> GetUserInfoFromTwitch(TwitchUserData twitchUserData)
    {
        _logger.LogDebug("Entered  GetUserInfoFromTwitch function");

        var twitchUserInfoResponse = await _getUserInfoFromTwitch.Execute(twitchUserData.UserAccessCode).ConfigureAwait(false);
        _logger.LogDebug("Returned from _getUserInfoFromTwitch.Execute" );
        
        if (!string.IsNullOrEmpty(twitchUserInfoResponse.ErrorCode))
        {
            twitchUserData.Error = twitchUserInfoResponse.ErrorCode;
            return twitchUserData;
        }
        
        if (!string.IsNullOrEmpty(twitchUserInfoResponse.TwitchId))
        {
            _logger.LogInformation($"Twitch returned userId: (private data)");
            twitchUserData.UserTwitchId = twitchUserInfoResponse.TwitchId;
        }
        else
        {
            _logger.LogWarning("Twitch failed to return user id");
            twitchUserData.Error = "AU000018";
        }
    
        if (!string.IsNullOrEmpty(twitchUserInfoResponse.Email))
        {
            _logger.LogInformation("Twitch returned User Email: (private data)");
            twitchUserData.UserTwitchEmail = twitchUserInfoResponse.Email;
        }
        else
        {
            _logger.LogWarning("Twitch failed to return email address");
            twitchUserData.Error = "AU000018";
        }
    
        if (!string.IsNullOrEmpty(twitchUserInfoResponse.PreferredUsername))
        {
            _logger.LogInformation($"Twitch returned Preferred Username: {twitchUserInfoResponse.PreferredUsername}");
            twitchUserData.TwitchUsername = twitchUserInfoResponse.PreferredUsername;
        }
        else
        {
            _logger.LogWarning("Twitch failed to return preferred username");
            twitchUserData.Error = "AU000018";
        }
    
        _logger.LogInformation($"Twitch returned EmailVerified: {twitchUserInfoResponse.EmailVerified}");
        twitchUserData.UserTwitchEmailVerified = twitchUserInfoResponse.EmailVerified;
        _logger.LogInformation($"Twitch return PictureUri: {twitchUserInfoResponse.PictureUri}");
        twitchUserData.PictureUri = twitchUserInfoResponse.PictureUri ?? string.Empty;
        
        return twitchUserData;
    }

    private async Task<Account> UpdateOrCreateAccountData(TwitchUserData twitchUserData)
    {
        _logger.LogDebug("Entered UpdateOrCreateAccountData function");
        var account = await _accountRepo.GetByTwitchUserIdAsync(twitchUserData.UserTwitchId);
        
        _logger.LogDebug("Returned from GetByTwitchUserIdAsync on AccountRepo");

        if (account != null)
        {
            _logger.LogDebug("Existing Account found. Updating user data. Calling UpdateAsync on AccountRepo.");
            var patchSucceeded = await _accountRepo.PatchTwitchData(account.Id,
                twitchUserData.UserAccessCode,
                twitchUserData.UserRefreshCode,
                twitchUserData.UserTwitchEmail,
                twitchUserData.UserTwitchEmailVerified,
                twitchUserData.TwitchUsername);
            if (!patchSucceeded)
            {
                return null;
            }
            account.Tokens.AccessCode = twitchUserData.UserAccessCode;
            account.Tokens.RefreshCode = twitchUserData.UserRefreshCode;
            account.TwitchEmail = twitchUserData.UserTwitchEmail;
            account.TwitchEmailVerified = twitchUserData.UserTwitchEmailVerified;
            account.TwitchUsername = twitchUserData.TwitchUsername;
            return account;
        }

        account = new Account()
        {
            Id = Guid.NewGuid().ToString(),
            TwitchUserId = twitchUserData.UserTwitchId,
            Tokens = new Account.TwitchTokens() { 
                AccessCode = twitchUserData.UserAccessCode,
                RefreshCode = twitchUserData.UserRefreshCode
            },
            TwitchEmail = twitchUserData.UserTwitchEmail,
            EmailAddress = twitchUserData.UserTwitchEmail,
            TwitchEmailVerified = twitchUserData.UserTwitchEmailVerified,
            TwitchUsername = twitchUserData.TwitchUsername,
            IsCreator = false,
            OnboardingStatus = 0,
            RestrictedStatus = 0,
            CreatedOn = DateTime.UtcNow
        };
        
        _logger.LogDebug("No existing account found. Creating new account.");
        _logger.LogInformation($"New account Id: {account.Id}");
        
        return await _accountRepo.CreateAsync(account);
    }

    private async Task<CreatorJwtFieldsModel> FetchCreatorDataFromAccount(string accountId)
    {
        _logger.LogDebug("Entered FetchCreatorStatusFromAccount function");
        var account = await _accountRepo.GetAsync(accountId);
        
        _logger.LogDebug("Returned from GetAsync on AccountRepo");

        if (account is null)
        {
            return new CreatorJwtFieldsModel()
            {
                ValidData = false
            };
        }
        
        _logger.LogDebug("Existing Account found.");
        _logger.LogInformation($"IsCreator: {account.IsCreator}");
        _logger.LogInformation($"OnboardingStatus: {account.OnboardingStatus}");
        _logger.LogInformation($"RestrictedStatus: {account.RestrictedStatus}");
        return new CreatorJwtFieldsModel()
        {
            ValidData = true,
            IsCreator = account.IsCreator,
            OnboardingStatus = account.OnboardingStatus,
            RestrictedStatus = account.RestrictedStatus
        };
    }

    private async Task<string> BuildJsonWebToken(JwtClaimsModel jwtClaims)
    {
        _logger.LogDebug("Entered BuildJsonWebToken function");
        _logger.LogInformation($"Account passed in Id: {jwtClaims.AccountId}");
        
        var jwtSettings = _config.Value.Jwt;
        
        var daysUntilExpiry = jwtSettings.DaysUntilExpiry;
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", jwtClaims.AccountId),
            new Claim("picture",  jwtClaims.PictureUri),
            new Claim("iscr", jwtClaims.IsCreator),
            new Claim("obstatus", jwtClaims.OnboardingStatus.ToString()),
            new Claim("resstatus", jwtClaims.RestrictedStatus.ToString())
        };
        var token = new JwtSecurityToken(jwtSettings.Issuer,
            jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(daysUntilExpiry),
            signingCredentials: credentials);

        _logger.LogDebug("Data setup for JWT");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}