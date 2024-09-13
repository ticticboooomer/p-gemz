using Stripe;

namespace Gemz.Api.Collector.Data.Model;

public class StripePaymentIntent : BaseDataModel
{
    public string PaymentIntentId { get; set; }
    public string MetadataOrderId { get; set; }
    public string MetadataCollectorId { get; set; }
    public long Amount { get; set; }
    public long ApplicationFeeAmount { get; set; }
    public string Currency { get; set; }
    public string LatestChargeId { get; set; }
    public string PaymentMethodId { get; set; }
    public DateTime PaymentIntentCreatedOn { get; set; }

    public string EventType { get; set; }
    public DateTime CreatedOn { get; set; }
}