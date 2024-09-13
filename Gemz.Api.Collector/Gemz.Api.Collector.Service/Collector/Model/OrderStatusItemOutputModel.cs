namespace Gemz.Api.Collector.Service.Collector.Model;

public class OrderStatusItemOutputModel
{
    public string? OrderLineId { get; set; }
    public string? StoreTag { get; set; }
    public string? CollectionId { get; set; }
    public string? CollectionName { get; set; }
    public int Quantity { get; set; }
    public float PackPrice { get; set; }
    public double LineTotal { get; set; }
}