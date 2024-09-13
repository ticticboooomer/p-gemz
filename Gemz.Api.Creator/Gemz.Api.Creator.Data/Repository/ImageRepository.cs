using Azure.Storage.Blobs;
using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Creator.Data.Repository;

public class ImageRepository : IImageRepository
{
    private readonly MongoFactory _dbFactory;
    private readonly ILogger<ImageRepository> _logger;
    private readonly IMongoCollection<Image> _dbContainer;
    private readonly BlobContainerClient _blobContainer;

    public ImageRepository(MongoFactory dbFactory, BlobFactory blobFactory, ILogger<ImageRepository> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        var db = _dbFactory.GetDatabase();
        _dbContainer = db.GetCollection<Image>("images");
        _blobContainer = blobFactory.GetBlobContainer();
    }

    public async Task<BlobFile> DownloadBlobFile(string blobFilename)
    {
        _logger.LogDebug("Entered GetBlobFile function");
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

    public async Task<BlobResponse> UploadBlobFile(IFormFile blob, string creatorId)
    {
        _logger.LogDebug("Entered UploadBlobFile function");

        var newFilename = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation($"Fetching client for {blob.FileName} with new name {newFilename}");
            var blobClient = _blobContainer.GetBlobClient(newFilename);

            await using (var data = blob.OpenReadStream())
            {
                _logger.LogDebug($"Beginning upload for {newFilename}");
                await blobClient.UploadAsync(data, true);
                _logger.LogDebug($"Finished upload for {newFilename}");
            }
        
            _logger.LogDebug("Creating Db images record for newly uploaded image");
            var imageEntity = new Image()
            {
                Id = newFilename,
                ContentType = blob.ContentType,
                CreatorId = creatorId,
                CreatedOn = DateTime.UtcNow,
                Deleted = false
            };
            await _dbContainer.InsertOneAsync(imageEntity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception occured whilst uploading file to Blob Storage or updating to Repo");
            return new BlobResponse()
            {
                Status = $"File {newFilename} failed to upload",
                Error = true,
                Filename = null
                
            };
        }
            
        _logger.LogDebug($"Returning response for blob {newFilename}");
        return new BlobResponse
        {
            Status = $"File {blob.FileName} Uploaded Successfully",
            Error = false,
            Filename = newFilename
        };
    }

    public async Task<Image> CreateAsync(Image entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Creating Image with id: {entity.Id}");

        try
        {
            await _dbContainer.InsertOneAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InsertOneAsync threw an exception");
            return null;
        }
        _logger.LogInformation($"InsertOneAsync succeeded");

        return entity;
    }

    public async Task<ImagesPage> GetImagesPageByCreatorId(string creatorId, int currentPage, int pageSize)
    {
        _logger.LogDebug("Entered GetImagesPageByCreatorId function");
        _logger.LogInformation($"Creator Id: {creatorId} | PageSize: {pageSize} ");

        try
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<Image, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<Image>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<Image, Image>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(Builders<Image>.Sort.Descending(x => x.CreatedOn)),
                PipelineStageDefinitionBuilder.Skip<Image>(currentPage == 0 ? 0 : (currentPage * pageSize)),
                PipelineStageDefinitionBuilder.Limit<Image>(pageSize),
                  }));

            var filter = Builders<Image>.Filter.Where(x => x.CreatorId == creatorId && x.Deleted == false);

            var aggregation = await _dbContainer.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count ?? 0;

            var totalPages = (int)count / pageSize;
            var remainder = (int)count % pageSize;
            if (remainder > 0) totalPages++;

            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<Image>();

            return new ImagesPage
            {
                Images  = new List<Image>(data),
                ThisPage = currentPage + 1,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repo error during fetch of Images for this CreatorId");
            return null;
        }
    }

    public async Task<Image?> FetchImageRecordByImageIdAndCreatorId(string imageId, string creatorId)
    {
        _logger.LogDebug("Entered FetchImageRecordByImageIdAndCreatorId function");
        _logger.LogInformation($"Creator id: {creatorId} | Image Id: {imageId}");


        var response = await _dbContainer.AsQueryable().FirstOrDefaultAsync(a => a.Id == imageId && a.CreatorId == creatorId && a.Deleted == false);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return an Image");
        }

        return response;
    }


    public async Task<bool> PatchImageAsDeleted(string imageId)
    {
        _logger.LogDebug("Entered PatchImageAsDeleted in repo.");

        var filter = Builders<Image>.Filter.Eq(c => c.Id, imageId);

        var update = Builders<Image>.Update.Set(c => c.Deleted, true);

        var resp = await _dbContainer.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? true : false;
    }
}