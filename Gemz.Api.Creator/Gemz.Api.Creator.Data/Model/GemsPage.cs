namespace Gemz.Api.Creator.Data.Model;

public class GemsPage
{
    public List<Gem> Gems { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }
}