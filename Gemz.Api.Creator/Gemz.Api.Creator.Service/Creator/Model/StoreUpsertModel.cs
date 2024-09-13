namespace Gemz.Api.Creator.Service.Creator.Model;

public class StoreUpsertModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string LogoImageId { get; set; }
    public string BannerImageId { get; set; }
    public int MaxAutoOpenRarity { get; set; }
    public DateTime? LiveDate { get; set; }
    public string UrlStoreTag { get; set; }
}