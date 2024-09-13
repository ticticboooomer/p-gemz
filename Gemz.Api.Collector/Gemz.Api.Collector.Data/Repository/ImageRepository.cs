using Azure.Storage.Blobs;
using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Collector.Data.Repository;

public class ImageRepository : IImageRepository
{
    private readonly BlobFactory _blobFactory;
    private readonly ILogger<ImageRepository> _logger;
    private readonly BlobContainerClient _blobContainer;
    
    public ImageRepository(BlobFactory blobFactory, ILogger<ImageRepository> logger)
    {
        _blobFactory = blobFactory;
        _logger = logger;
        _blobContainer = blobFactory.GetBlobContainer();
    }
    
    public async Task<BlobFile> DownloadBlobFile(string blobFilename)
    {
        _logger.LogDebug("Entered DownloadBlobFile function");
        _logger.LogInformation($"Blob Filename: {blobFilename}");
        
        var file = _blobContainer.GetBlobClient(blobFilename);

        if (!await file.ExistsAsync())
        {
            _logger.LogWarning("Blob file does not exist in Blob Storage Container");
            return null;
        }

        _logger.LogDebug("Found file in Blob Storage container. Continuing to Download.");
        var data = await file.OpenReadAsync();

        if (data is null)
        {
            _logger.LogWarning("Unable to read data from Blob Storage Container. Exist function.");
            return null;
        }

        var content = await file.DownloadContentAsync();
        
        _logger.LogDebug("Downloaded data and content Info. Returning to caller.");
        return new BlobFile()
        {
            Content = data,
            ContentType = content.Value.Details.ContentType,
            Name = blobFilename
        };
    }
}