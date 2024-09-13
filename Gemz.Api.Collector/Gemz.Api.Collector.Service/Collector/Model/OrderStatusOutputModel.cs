using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Service.Collector.Model;

public class OrderStatusOutputModel
{
    public string OrderId { get; set; }
    public string CollectorId { get; set; }
    public string OriginatingBasketId { get; set; }
    public OrderStatus Status { get; set; }
    public string PaymentIntentClientSecret { get; set; }
    public double OrderTotal { get; set; }
    public List<OrderStatusItemOutputModel> Items { get; set; }
}