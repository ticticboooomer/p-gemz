namespace Gemz.Api.Auth.Service.Accounts.Model;

public class AccountModel
{
    public string EmailAddress { get; set; }
    public bool IsCreator { get; set; }
    public string TwitchUsername { get; set; }
    public int OnboardingStatus { get; set; }
}