using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gemz.Api.Auth.Data.Model;
using Gemz.Api.Auth.Service.Auth.Model;
using Gemz.Api.Auth.Test.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;

namespace Gemz.Api.Auth.Test;

public class TwitchAuthService_Setups
{

    private AuthConfig GetAuthConfig()
    {
        return new AuthConfig()
        {
            Twitch = GetTwitchConfigData(),
            Jwt = GetJwtConfigData()
        };
    }
    
    public CallbackParams SetUpCallbackParams(string accessCode, string state, string error, string errorDescription)
    {
        return new CallbackParams()
        {
            AccessCode = accessCode,
            State = state,
            Error = error,
            ErrorDescription = errorDescription
        };
    }
    
    public AuthState SetUpAuthStateData()
    {
        return new AuthState()
        {
            Id = Guid.NewGuid().ToString(),
            Provider = "Twitch",
            ReturnUri = "https://mocking.com/"
        };
    }
    
    public Account SetUpAccountData()
    {
        return new Account()
        {
            Id = Guid.NewGuid().ToString(),
            TwitchEmail = "test@mockme.com",
            TwitchEmailVerified = true,
            TwitchUserId = Guid.NewGuid().ToString(),
            TwitchUsername = "MockingMeYouAre",
            IsCreator = false,
            Tokens = new Account.TwitchTokens()
            {
                AccessCode = Guid.NewGuid().ToString(),
                RefreshCode = Guid.NewGuid().ToString()
            }
        };
    }

    public TwitchUserData SetupTwitchUserDataResponse()
    {
        return new TwitchUserData()
        {
            UserAccessCode = Guid.NewGuid().ToString(),
            UserRefreshCode = Guid.NewGuid().ToString(),
        };
    }

    public TwitchUserData SetupTwitchUserDataResponseError(string errorCode)
    {
        return new TwitchUserData()
        {
            Error = errorCode
        };
    }

    public TwitchUserInfoResponse SetupTwitchUserInfoResponseError(string errorCode)
    {
        return new TwitchUserInfoResponse()
        {
            ErrorCode = errorCode
        };
    }
    
    public TwitchUserInfoResponse SetupTwitchUserInfoResponse()
    {
        return new TwitchUserInfoResponse()
        {
            PictureUri = "https://piccys.com/iamapiccy.jpg",
            Email = "test@myemail.com",
            EmailVerified = true,
            PreferredUsername = "TwitchUsernameTest",
            TwitchId = "1234567890"
        };
    }

    public TwitchAuthCodeResponse SetupTwitchAuthCodeResponse()
    {
        return new TwitchAuthCodeResponse()
        {
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString()
        };
    }

    public TwitchAuthCodeResponse SetupTwitchAuthCodeResponseError(string errorCode)
    {
        return new TwitchAuthCodeResponse()
        {
            ErrorCode = errorCode
        };
    }

    public Mock<IOptions<AuthConfig>> SetUpAuthConfig()
    {
        var config = new Mock<IOptions<AuthConfig>>();
        config.Setup(a => a.Value).Returns(GetAuthConfig());
        return config;
    }

    public string SetupTestJwt(string accountId, AuthConfig config)
    {
        var daysUntilExpiry = config.Jwt.DaysUntilExpiry;
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Jwt.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", accountId),
            new Claim("picture", ""),
            new Claim("iscr", "false"),
            new Claim("obstatus","0"),
            new Claim("resstatus", "0")
        };
        var token = new JwtSecurityToken(config.Jwt.Issuer,
            config.Jwt.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(daysUntilExpiry),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string[] CheckValidJwt(string token, AuthConfig config)
    {
        var handler = new JwtSecurityTokenHandler();
        
        try
        {
            var ret = handler.ValidateToken(token, new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Jwt.Key)),
                ValidateIssuer = true,
                ValidIssuer = config.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = config.Jwt.Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            var returnStrings = new string[3];
            returnStrings[0] = jwtToken.Claims.First(x => x.Type == "sub").Value;
            returnStrings[1] = jwtToken.Claims.First(x => x.Type == "iscr").Value;
            returnStrings[2] = jwtToken.Claims.First(x => x.Type == "obstatus").Value;
            return returnStrings;
        }
        catch
        {
            return null;
        }
    }


    private Mock<HttpMessageHandler> SetupSendAsyncHandlerMock(Mock<HttpMessageHandler> handlerMock,
        HttpResponseMessage mockHttpResponseMessage, 
        HttpRequestMessage mockHttpRequestMessage)
    {
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == mockHttpRequestMessage.Method 
                                                     && req.RequestUri == mockHttpRequestMessage.RequestUri), 
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockHttpResponseMessage);

        return handlerMock;
    }
    

    private TwitchConfig GetTwitchConfigData()
    {
        return new TwitchConfig()
        {
            ClientId = "dogseatrandomsausages",
            Scope = "openid user:read:email",
            UrlStart = "https://id.twitch.tv/oauth2/authorize",
            TokenEndpoint = "https://id.twitch.tv/oauth2/token",
            UserInfoEndpoint = "https://id.twitch.tv/oauth2/userinfo",
            TokenValidateEndpoint = "https://id.twitch.tv/oauth2/validate",
            ClientSecret = "ihaveasecretandyoudontknowit",
            RedirectUrl = "https://localhost:7212/api/auth/twitch/callback",
            DefaultErrorUrl = "https://localhost:7212/default/error"
        };
    }
    
    private JwtConfig GetJwtConfigData()
    {
        return new JwtConfig()
        {
            Key = "sausagebuttiesarefab",
            Issuer = "http://localhost:7212/",
            Audience = "http://localhost:7212/",
            DaysUntilExpiry = 7
        };
    }
}