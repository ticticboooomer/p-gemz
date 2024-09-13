namespace Gemz.Api.Collector.Data.Model;

public class Order : BaseDataModel
{
    public string CollectorId { get; set; }
    public string OriginatingBasketId { get; set; }
    public string CreatorId { get; set; }
    public OrderStatus Status { get; set; }
    public string PaymentIntentClientSecret { get; set; }
    public string PaymentConnectedStripeAccount { get; set; }
    public string StripeErrorMessage { get; set; }
    public double OrderTotal { get; set; }
    public double CommissionAmount { get; set; }
    public List<OrderItem> Items { get; set; }
    public DateTime CreatedOn { get; set; }
}
