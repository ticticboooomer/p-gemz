using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;

public interface IImageRepository
{
    Task<BlobFile> DownloadBlobFile(string blobFilename);    
}