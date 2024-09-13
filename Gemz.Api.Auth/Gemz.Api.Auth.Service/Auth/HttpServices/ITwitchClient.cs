using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public interface ITwitchClient
{
    Task<TwitchUserData> GetTwitchAccessTokens(string code);
    Task<TwitchUserInfoResponse> GetUserInfoFromTwitch(string twitchUserAccessCode);

    Task<bool> ValidateTwitchAccessToken(string accessToken);

    Task<TwitchAuthCodeResponse> RefreshTokenFromTwitch(string twitchRefreshCode);
}