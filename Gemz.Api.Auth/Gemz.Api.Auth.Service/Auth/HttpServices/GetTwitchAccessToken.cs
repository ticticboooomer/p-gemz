using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public class GetTwitchAccessToken : IGetTwitchAccessToken
{
    private readonly TwitchClientFactory _twitchClientFactory;

    public GetTwitchAccessToken(TwitchClientFactory twitchClientFactory)
    {
        _twitchClientFactory = twitchClientFactory;
    }
    
    public async Task<TwitchUserData> Execute(string code)
    {
        var twitchClient = _twitchClientFactory.Create();
        return await twitchClient.GetTwitchAccessTokens(code);
    }
}