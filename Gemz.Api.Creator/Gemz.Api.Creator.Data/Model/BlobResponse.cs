using System.Reflection.Metadata;

namespace Gemz.Api.Creator.Data.Model;

public class BlobResponse
{
    public string Status { get; set; }
    public bool Error { get; set; }
    public string Filename { get; set; }
}