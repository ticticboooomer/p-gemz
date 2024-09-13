namespace Gemz.Api.Collector.Service.Collector.Model;

public class GemsPagedModel
{
    public List<GemModel>? Gems { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }

}