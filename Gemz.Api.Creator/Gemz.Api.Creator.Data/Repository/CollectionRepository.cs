using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Creator.Data.Repository;

public class CollectionRepository : ICollectionRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<CollectionRepository> _logger;
    private readonly IMongoCollection<Collection> _container;

    public CollectionRepository(MongoFactory factory, ILogger<CollectionRepository> logger)
    {
        this._factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Collection>("collections");
    }

    public async Task<Collection> CreateAsync(Collection entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Creating Collection with id: {entity.Id}");

        try
        {
            await _container.InsertOneAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InsertOneAsync threw an exception");
            return null;
        }
        _logger.LogInformation($"InsertOneAsync succeeded");

        return entity;
    }

    public async Task<Collection> GetAsync(string collectionId)
    {
        _logger.LogDebug("Entered GetAsync function");
        _logger.LogInformation($"Collection id: {collectionId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.Id == collectionId && x.Deleted == false);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<bool> PatchItemPublishedStatusAsync(string collectionId, int publishedStatus)
    {
        _logger.LogDebug("Entered PatchItemPublishedStatusAsync in repo.");

        var filter = Builders<Collection>.Filter.Eq(c => c.Id, collectionId);

        var update = Builders<Collection>.Update.Set(c => c.PublishedStatus, publishedStatus);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? true : false;
    }

    public async Task<Collection> UpdateCollectionAsync(Collection entity)
    {
        _logger.LogDebug("Entered UpdateCollectionAsync in repo.");
        _logger.LogInformation($"Collection entity Id: {entity.Id}");

        var update = Builders<Collection>.Update.Set(x => x.Name, entity.Name)
            .Set(x => x.ClusterPrice, entity.ClusterPrice)
            .Set(x => x.PublishedStatus, entity.PublishedStatus)
            .Set(x => x.Deleted, entity.Deleted);
        
        var resp = await _container.UpdateOneAsync(x => x.Id == entity.Id, update);

        _logger.LogDebug("Returned from ReplaceOneAsync call");
        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? entity : null;
    }

    public async Task<bool> PatchCollectionDeletedAsync(string collectionId)
    {
        _logger.LogDebug("Entered PatchCollectionDeletedAsync in repo.");

        var filter = Builders<Collection>.Filter.Eq(c => c.Id, collectionId);

        var update = Builders<Collection>.Update.Set(c => c.Deleted, true);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? true : false;
    }

    public async Task<CollectionsPage> GetCollectionsPageByCreatorId(string creatorId, int currentPage, int pageSize)
    {
        _logger.LogDebug("Entered GetCollectionsPageByCreatorId function");
        _logger.LogInformation($"Creator Id: {creatorId} | PageSize: {pageSize}");

        try
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<Collection, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<Collection>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<Collection, Collection>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(Builders<Collection>.Sort.Ascending(x => x.CreatedOn)),
                PipelineStageDefinitionBuilder.Skip<Collection>(currentPage == 0 ? 0 : (currentPage * pageSize)),
                PipelineStageDefinitionBuilder.Limit<Collection>(pageSize),
                  }));

            var filter = Builders<Collection>.Filter.Where(x => x.CreatorId == creatorId && x.Deleted == false);

            var aggregation = await _container.Aggregate()
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
                .Output<Collection>();

            return new CollectionsPage
            {
                Collections = new List<Collection>(data),
                ThisPage = currentPage + 1,
                TotalPages = totalPages
            };
        }
        catch(Exception ex) {
            _logger.LogWarning(ex, "Repo error during fetch of Collections for this CreatorId");
            return null;
        }
    }

    public async Task<List<Collection>> GetAllCollectionsByCreatorId(string creatorId)
    {
        _logger.LogDebug("Entered GetAllCollectionsByCreatorId function.");

        var filter = Builders<Collection>.Filter.Where(a => a.CreatorId == creatorId && a.Deleted == false);

        return await _container.Find(filter).ToListAsync();
    }

    public async Task<Collection> GetAsyncAnyStatus(string collectionId)
    {
        _logger.LogDebug("Entered GetAsync function");
        _logger.LogInformation($"Collection id: {collectionId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.Id == collectionId);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;

    }
}   

