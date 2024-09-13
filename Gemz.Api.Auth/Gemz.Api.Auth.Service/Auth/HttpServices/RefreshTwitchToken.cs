using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public class RefreshTwitchToken : IRefreshTwitchToken
{
    private readonly TwitchClientFactory _twitchClientFactory;

    public RefreshTwitchToken(TwitchClientFactory twitchClientFactory)
    {
        _twitchClientFactory = twitchClientFactory;
    }
    
    public async Task<TwitchAuthCodeResponse> Execute(string twitchRefreshCode)
    {
        var twitchClient = _twitchClientFactory.Create();
        return await twitchClient.RefreshTokenFromTwitch(twitchRefreshCode);
    }
}