
namespace Gemz.Api.Creator.Service.Creator.Model;

public class ImageModel
{
    public string Uri { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    public Stream Content { get; set; }
}