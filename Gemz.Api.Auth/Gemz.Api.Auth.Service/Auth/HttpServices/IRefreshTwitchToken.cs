using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public interface IRefreshTwitchToken
{
    Task<TwitchAuthCodeResponse> Execute(string twitchRefreshCode);
}