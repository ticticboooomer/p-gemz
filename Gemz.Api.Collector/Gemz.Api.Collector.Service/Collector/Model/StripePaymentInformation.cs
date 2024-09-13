namespace Gemz.Api.Collector.Service.Collector.Model;

public class StripePaymentInformation
{
    public string? ClientSecret { get; set; }
    public string PaymentConnectedAccount { get; set; }
}