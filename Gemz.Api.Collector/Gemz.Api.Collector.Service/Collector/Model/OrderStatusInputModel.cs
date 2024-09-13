namespace Gemz.Api.Collector.Service.Collector.Model;

public class OrderStatusInputModel
{
    public string? SearchKeyData { get; set; }
    public string? SearchKeyUsed { get; set; }
    public int NewOrderStatus { get; set; }
}