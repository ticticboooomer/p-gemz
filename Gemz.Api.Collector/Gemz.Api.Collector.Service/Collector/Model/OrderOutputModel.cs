using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Service.Collector.Model;

public class OrderOutputModel
{
    public string OrderId { get; set; }
    public string CollectorId { get; set; }
    public string OriginatingBasketId { get; set; }
    public OrderStatus Status { get; set; }
    public string PaymentIntentClientSecret { get; set; }
    public string StripeErrorMessage { get; set; }
    public double OrderTotal { get; set; }
    public string OrderDate { get; set; }
    public string OrderTime { get; set; }
    public string StoreTag { get; set; }
    public string StoreName { get; set; }
    public string StoreLogoImageId { get; set; }
    public List<OrderOutputItem> Items { get; set; }
}