namespace Gemz.Api.Auth.Service.Auth.Model;

public class AuthConfig
{
    public TwitchConfig Twitch { get; set; }
    public JwtConfig Jwt { get; set; }
}