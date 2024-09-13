namespace Gemz.Api.Collector.Service.Collector.Model;

public class CollectionPagingModel
{
    public string StoreTag { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
}