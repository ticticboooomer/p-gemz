namespace Gemz.Api.Collector.Data.Model;

public class CollectionsPage
{
    public List<Collection> Collections { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }
}