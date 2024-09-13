namespace Gemz.Api.Collector.Service.Collector.Model;

public class BasketModel
{
    public string BasketId { get; set; }
    public string StoreTag { get; set; }
    public string StoreName { get; set; }
    public string StoreLogoImageId { get; set; }
    public List<BasketItemModel>? Items { get; set; }
}