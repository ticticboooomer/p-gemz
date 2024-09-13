using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Auth;

public interface ITwitchAuthService
{
    Task<TwitchAuthorizeResultModel> Authorize(string returnUri);
    
    Task<TwitchAuthorizeResultModel> Callback(string accessCode, string state, string error, string errorDescription);

    Task<GenericResponse<JwtResultModel>> Refresh(string token);

    Task<GenericResponse<ValidateResultModel>> Validate(string token);
}