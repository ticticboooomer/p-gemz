namespace Gemz.Api.Collector.Service.Collector.Model;

public class CollectionsPagedModel
{
    public List<CollectionModel>? Collections { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }
}