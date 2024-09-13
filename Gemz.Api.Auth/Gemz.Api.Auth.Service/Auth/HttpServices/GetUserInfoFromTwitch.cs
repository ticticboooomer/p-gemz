using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public class GetUserInfoFromTwitch : IGetUserInfoFromTwitch
{
    private readonly TwitchClientFactory _twitchClientFactory;

    public GetUserInfoFromTwitch(TwitchClientFactory twitchClientFactory)
    {
        _twitchClientFactory = twitchClientFactory;
    }
    
    public async Task<TwitchUserInfoResponse> Execute(string twitchUserAccessCode)
    {
        var twitchClient = _twitchClientFactory.Create();
        return await twitchClient.GetUserInfoFromTwitch(twitchUserAccessCode);
    }
}