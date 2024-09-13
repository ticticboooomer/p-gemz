namespace Gemz.Api.Creator.Service.Creator.Model;

public class GemCollectionModel
{
    public List<GemModel> Gems { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }
}