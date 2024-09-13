namespace Gemz.Api.Collector.Data.Model;

public class Store : BaseDataModel
{
    public string CreatorId { get; set; }
    public string Name { get; set; }
    public string? LogoImageId { get; set; }
    public string? BannerImageId { get; set; }
    public int MaxAutoOpenRarity { get; set; }
    public DateTime LiveDate { get; set; }
    public string UrlStoreTag { get; set; }
    public DateTime CreatedOn { get; set; }
    
}