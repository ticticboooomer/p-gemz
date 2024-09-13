namespace Gemz.Api.Auth.Test.Model;

public class HttpSetupType
{
    public enum SetupVariation
    {
        OkWithTokens,
        NotOkNoTokens,
        BadRequestUserInfo,
        BadRequestValidate,
        OkNoData,
        OkWithUserData,
        OkWithUserIdMissing,
        OkWithUserEmailMissing,
        OkWithPreferredUsernameMissing
    }
}