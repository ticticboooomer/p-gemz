namespace Gemz.Api.Collector.Data.Model;

public class Basket : BaseDataModel
{
    public string CollectorId { get; set; }
    public string CreatorId { get; set; }
    public List<BasketItem> Items { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedOn { get; set; }
}