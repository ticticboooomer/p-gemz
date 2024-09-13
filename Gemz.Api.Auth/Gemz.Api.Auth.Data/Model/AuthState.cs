namespace Gemz.Api.Auth.Data.Model;

public class AuthState : BaseDataModel
{
    public string ReturnUri { get; set; }
    public string Provider { get; set; }
}