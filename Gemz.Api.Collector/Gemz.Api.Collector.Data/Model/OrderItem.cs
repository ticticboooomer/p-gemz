namespace Gemz.Api.Collector.Data.Model;

public class OrderItem : BaseDataModel
{
    public string CollectionId { get; set; }
    public int Quantity { get; set; }
    public float PackPrice { get; set; }
    public double LineTotal { get; set; }
}