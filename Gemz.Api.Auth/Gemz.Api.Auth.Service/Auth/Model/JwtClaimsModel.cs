namespace Gemz.Api.Auth.Service.Auth.Model;

public class JwtClaimsModel
{
    public string AccountId { get; set; }
    public string PictureUri { get; set; }
    public string IsCreator { get; set; }
    public int OnboardingStatus { get; set; }
    public int RestrictedStatus { get; set; }
}