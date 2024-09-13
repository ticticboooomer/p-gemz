namespace Gemz.Api.Creator.Data.Model;

public class ImagesPage
{
    public List<Image> Images { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }
}