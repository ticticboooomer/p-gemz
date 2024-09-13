using System.Text.Json.Serialization;

namespace Gemz.Api.Auth.Service.Auth.Model;

public class UserInfoClaimsModel
{
    [JsonPropertyName("userinfo")]
    public UserInfoClaims UserInfo { get; set; }
    
    public class UserInfoClaims 
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("email_verified")]
        public string EmailVerified { get; set; }

        [JsonPropertyName("preferred_username")]
        public string TwitchUsername { get; set; }

        [JsonPropertyName("picture")]
        public string PictureUri { get; set; }
    }
}