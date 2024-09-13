using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Collector.Service.Collector;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepo;
    private readonly ILogger<CollectionService> _logger;

    public ImageService(IImageRepository imageRepo, ILogger<CollectionService> logger)
    {
        _imageRepo = imageRepo;
        _logger = logger;
    }
    
    public async Task<BlobFile?> FetchImageFromStorage(string imageId)
    {
        _logger.LogDebug("Entered FetchImageFromStorage function");
        _logger.LogInformation($"ImageId: {imageId}");

        if (string.IsNullOrEmpty(imageId))
        {
            _logger.LogError("Image Id was null or Empty. Exiting function.");
            return null;
        }

        var blobFile = await _imageRepo.DownloadBlobFile(imageId);

        if (blobFile is null)
        {
            _logger.LogError("Unable to retrieve blob from storage");
            return null;
        }

        return blobFile;
    }
}