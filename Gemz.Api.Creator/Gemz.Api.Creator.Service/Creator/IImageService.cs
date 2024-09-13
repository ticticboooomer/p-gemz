using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Http;

namespace Gemz.Api.Creator.Service.Creator;

public interface IImageService
{
    Task<GenericResponse<ImagePageResponse>> FetchPageOfImages(ImagesPagingModel imagesPagingModel, string creatorId);

    Task<BlobFile> FetchImageFromStorage(string imageId);
    
    Task<GenericResponse<List<ImageResponse>>> UploadImagesToStorage(IFormFile[] files, string creatorId);

    Task<GenericResponse<bool>> ArchiveImage(ImageIdModel imageIdModel, string creatorId);
}