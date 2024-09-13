namespace Gemz.Api.Auth.Service.Auth.Model;

public class JwtConfig
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int DaysUntilExpiry { get; set; }
}