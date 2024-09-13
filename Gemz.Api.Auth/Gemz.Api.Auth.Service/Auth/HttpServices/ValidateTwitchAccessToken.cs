namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public class ValidateTwitchAccessToken : IValidateTwitchAccessToken
{
    private readonly TwitchClientFactory _twitchClientFactory;

    public ValidateTwitchAccessToken(TwitchClientFactory twitchClientFactory)
    {
        _twitchClientFactory = twitchClientFactory;
    }
    
    public async Task<bool> Execute(string accessToken)
    {
        var twitchClient = _twitchClientFactory.Create();
        return await twitchClient.ValidateTwitchAccessToken(accessToken);
    }
}