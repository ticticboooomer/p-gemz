using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public interface IGetUserInfoFromTwitch
{
    Task<TwitchUserInfoResponse> Execute(string twitchUserAccessCode);
}