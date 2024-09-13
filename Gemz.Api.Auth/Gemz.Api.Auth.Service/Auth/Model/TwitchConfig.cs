namespace Gemz.Api.Auth.Service.Auth.Model;

public class TwitchConfig
{
    public string ClientId { get; set; }
    public string Scope { get; set; }
    public string UrlStart { get; set; }
    public string TokenEndpoint { get; set; }
    public string UserInfoEndpoint { get; set; }
    public string TokenValidateEndpoint { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUrl { get; set; }
    public string DefaultErrorUrl { get; set; }

}