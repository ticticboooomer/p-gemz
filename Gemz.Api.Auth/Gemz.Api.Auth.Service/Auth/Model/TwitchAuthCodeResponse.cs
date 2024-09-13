using System.Text.Json.Serialization;

namespace Gemz.Api.Auth.Service.Auth.Model;

public class TwitchAuthCodeResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    public string ErrorCode { get; set; }
}