namespace Gemz.Api.Auth.Test.Model;

public class CallbackParams
{
    public string AccessCode { get; set; }
    public string State { get; set; }
    public string Error { get; set; }
    public string ErrorDescription { get; set; }
}