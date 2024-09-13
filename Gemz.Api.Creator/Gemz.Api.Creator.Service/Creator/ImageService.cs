using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Creator.Service.Creator;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepo;
    private readonly ILogger<ImageService> _logger;

    public ImageService(IImageRepository imageRepo, ILogger<ImageService> logger)
    {
        _imageRepo = imageRepo;
        _logger = logger;
        
    }

    public async Task<BlobFile> FetchImageFromStorage(string imageId)
    {
        _logger.LogDebug("Entered FetchImageFromStorage function");
        _logger.LogInformation($"ImageId: {imageId}");

        if (string.IsNullOrEmpty(imageId))
        {
            _logger.LogWarning("Image Id was null or Empty. Exiting function.");
            return null;
        }

        var blobFile = await _imageRepo.DownloadBlobFile(imageId);

        if (blobFile == null)
        {
            _logger.LogWarning("Unable to retrieve blob from storage");
            return null;
        }

        return blobFile;
    }

    public async Task<GenericResponse<List<ImageResponse>>> UploadImagesToStorage(IFormFile[] files, string creatorId)
    {
        _logger.LogDebug("Entered UploadImagesToStorage function");

        if (files == null)
        {
            _logger.LogWarning("No files passed in. Param was null. Exiting function.");
            return new GenericResponse<List<ImageResponse>>()
            {
                Error = "CR000200"
            };
        }

        if (files.Length == 0)
        {
            _logger.LogWarning("No files passed in. Param was zero length. Exiting function.");
            return new GenericResponse<List<ImageResponse>>()
            {
                Error = "CR000201"
            };
        }

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogWarning("No creatorId passed in. Exiting function.");
            return new GenericResponse<List<ImageResponse>>()
            {
                Error = "CR000203"
            };
        }

        var response = new GenericResponse<List<ImageResponse>>()
        {
            Data = new List<ImageResponse>()
        };

        var counter = 0;
        foreach (var formFile in files)
        {
            counter++;
            var result = await _imageRepo.UploadBlobFile(formFile, creatorId);

            if (result.Error)
            {
                response.Data.Add(new ImageResponse()
                {
                    FileIndex = counter,
                    Error = "CR000202"
                });
            }
            else
            {
                response.Data.Add(new ImageResponse()
                    {
                        FileIndex = counter,
                        ImageId = result.Filename
                    });
            }
        }

        return response;
    }

    public async Task<GenericResponse<bool>> ArchiveImage(ImageIdModel imageIdModel, string creatorId)
    {
        _logger.LogDebug("Entered ArchiveImage function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogWarning("Creator Id is missing");
            return new GenericResponse<bool>()
            {
                Error = "CR000800"
            };
        }

        if (string.IsNullOrEmpty(imageIdModel.ImageId))
        {
            _logger.LogWarning("Image Id Parameter is missing");
            return new GenericResponse<bool>()
            {
                Error = "CR000801"
            };
        }

        var existingImage = await _imageRepo.FetchImageRecordByImageIdAndCreatorId(imageIdModel.ImageId, creatorId);

        if (existingImage is null)
        {
            _logger.LogWarning($"Unable to find Image with ImageId {imageIdModel.ImageId} for creator");
            return new GenericResponse<bool>()
            {
                Error = "CR000802"
            };
        }

        existingImage.Deleted = true;
        
        var success = await _imageRepo.PatchImageAsDeleted(imageIdModel.ImageId);

        if (!success)
        {
            _logger.LogWarning("Error occured in Repo During Patch Image as Deleted");
            return new GenericResponse<bool>()
            {
                Error = "CR000804"
            };
        }

        return new GenericResponse<bool>()
        {
            Data = true
        };
    }

    public async Task<GenericResponse<ImagePageResponse>> FetchPageOfImages(ImagesPagingModel imagesPagingModel, string creatorId)
    {
        _logger.LogDebug("Entered FetchPageOfImages function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogWarning("No creatorId passed in. Exiting function.");
            return new GenericResponse<ImagePageResponse>()
            {
                Error = "CR000300"
            };
        }
        
        if (imagesPagingModel is null)
        {
            _logger.LogWarning("Null ImagesPagingModel passed in. Exiting function.");
            return new GenericResponse<ImagePageResponse>()
            {
                Error = "CR000301"
            };
        }
        if (imagesPagingModel.CurrentPage < 0)
        {
            _logger.LogWarning("currentPage passed in was invalid (< 0). Leaving function.");
            return new GenericResponse<ImagePageResponse>()
            {
                Error = "CR000302"
            };
        }

        if (imagesPagingModel.PageSize <= 0)
        {
            _logger.LogWarning("pageSize passed in as <= zero, leaving function.");
            return new GenericResponse<ImagePageResponse>()
            {
                Error = "CR000303"
            };
        }

        _logger.LogDebug("Calling Images Repo GetImagesPageByCreatorId");
        var imagesPage = await _imageRepo.GetImagesPageByCreatorId(creatorId, imagesPagingModel.CurrentPage, imagesPagingModel.PageSize);

        if (imagesPage == null)
        {
            _logger.LogWarning(
                "null returned from repo function GetImagesPageByCreatorId. Leaving function, returning null.");
            return new GenericResponse<ImagePageResponse>()
            {
                Error = "CR000304"
            };
        }
        
        _logger.LogInformation($"Repo returned {imagesPage.Images.Count} image records.");
        var imagePageResponse = new ImagePageResponse()
        {
            ImageIds = new List<string>(),
            ThisPage = imagesPage.ThisPage,
            TotalPages = imagesPage.TotalPages
        };

        _logger.LogDebug("Building ImageId List from repo data.");
        foreach (var image in imagesPage.Images)
        {
            imagePageResponse.ImageIds.Add(image.Id);
        }
        
        _logger.LogDebug("Return data built, returning data.");
        return new GenericResponse<ImagePageResponse>()
        {
            Data = imagePageResponse
        };
    }
}