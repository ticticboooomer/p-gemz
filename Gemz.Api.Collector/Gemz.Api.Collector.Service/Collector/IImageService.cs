using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Service.Collector;

public interface IImageService
{
    Task<BlobFile?> FetchImageFromStorage(string imageId);
    
}