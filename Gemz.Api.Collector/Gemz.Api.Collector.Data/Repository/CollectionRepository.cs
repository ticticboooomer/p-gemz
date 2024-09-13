using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository;

public class CollectionRepository : ICollectionRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<CollectionRepository> _logger;
    private readonly IMongoCollection<Collection> _container;

    public CollectionRepository(MongoFactory factory, ILogger<CollectionRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Collection>("collections");
    }

    public async Task<List<Collection>> GetAllCollectionsByCreatorId(string creatorId)
    {
        _logger.LogDebug("Entered GetAllCollectionsByCreatorId function");
        _logger.LogInformation($"Creator Id: {creatorId}");

        var filter = Builders<Collection>.Filter.Where(x => x.CreatorId == creatorId && x.Deleted == false && x.PublishedStatus == 1);

        return await _container.Find(filter).ToListAsync();
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

            var filter = Builders<Collection>.Filter.Where(x => x.CreatorId == creatorId && x.Deleted == false && x.PublishedStatus == 1);

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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repo error during fetch of Collections for this CreatorId");
            return null;
        }
    }

    public async Task<Collection> GetSingleCollection(string collectionId)
    {
        _logger.LogDebug("Entered GetSingleCollection function");
        _logger.LogInformation($"CollectionId: {collectionId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == collectionId && a.PublishedStatus == 1 && a.Deleted == false);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<Collection> GetSingleCollectionAnyStatus(string collectionId)
    {
        _logger.LogDebug("Entered GetSingleCollectionAnyStatus function");
        _logger.LogInformation($"CollectionId: {collectionId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == collectionId && a.Deleted == false);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }
}