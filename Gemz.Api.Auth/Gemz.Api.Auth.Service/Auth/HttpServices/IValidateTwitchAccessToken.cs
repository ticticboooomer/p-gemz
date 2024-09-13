namespace Gemz.Api.Auth.Service.Auth.HttpServices;

public interface IValidateTwitchAccessToken
{
    Task<bool> Execute(string accessToken);
}