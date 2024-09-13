using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public interface IGetTwitchAccessToken
{
    Task<TwitchUserData> Execute(string code);
}