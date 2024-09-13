namespace Gemz.Api.Collector.Service.Collector.Model;

public class BasketItemUpdateModel
{
    public string BasketId { get; set; }
    public string LineId { get; set; }
    public int Quantity { get; set; }
}