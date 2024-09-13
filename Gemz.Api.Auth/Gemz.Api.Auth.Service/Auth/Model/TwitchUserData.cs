namespace Gemz.Api.Auth.Service.Auth.Model;

public class TwitchUserData
{
    public string UserAccessCode { get; set; }
    public string UserRefreshCode { get; set; }
    public string UserTwitchId { get; set; }
    public string UserTwitchEmail { get; set; }
    public bool UserTwitchEmailVerified { get; set; }
    public string TwitchUsername { get; set; }
    
    public string PictureUri { get; set; }
    public string? Error { get; set; }
}