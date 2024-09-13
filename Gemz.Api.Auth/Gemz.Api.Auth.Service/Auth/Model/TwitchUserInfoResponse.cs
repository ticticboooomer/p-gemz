using System.Text.Json.Serialization;

namespace Gemz.Api.Auth.Service.Auth.Model;

public class TwitchUserInfoResponse
{
    [JsonPropertyName("sub")]
    public string TwitchId { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; set; }
    
    [JsonPropertyName("preferred_username")]
    public string PreferredUsername { get; set; }
    
    [JsonPropertyName("picture")]
    public string PictureUri { get; set; }

    public string ErrorCode { get; set; }
}