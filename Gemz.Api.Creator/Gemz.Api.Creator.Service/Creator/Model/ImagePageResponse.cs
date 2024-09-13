namespace Gemz.Api.Creator.Service.Creator.Model;

public class ImagePageResponse
{
    public List<string> ImageIds { get; set; }
    public int ThisPage { get; set; }
    public int TotalPages { get; set; }
}