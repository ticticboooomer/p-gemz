using Gemz.Api.Creator.Data.Model;
using Microsoft.AspNetCore.Http;

namespace Gemz.Api.Creator.Data.Repository;

public interface IImageRepository
{
    Task<ImagesPage> GetImagesPageByCreatorId(string creatorId, int currentPage, int pageSize);
    Task<Image> CreateAsync(Image entity);
    
    Task<BlobFile> DownloadBlobFile(string blobFilename);
    
    Task<BlobResponse> UploadBlobFile(IFormFile blob, string creatorId);
    Task<Image> FetchImageRecordByImageIdAndCreatorId(string imageId, string creatorId);

    Task<bool> PatchImageAsDeleted(string imageId);
}