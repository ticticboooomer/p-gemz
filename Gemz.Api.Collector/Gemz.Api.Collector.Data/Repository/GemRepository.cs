using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository;

public class GemRepository : IGemRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<GemRepository> _logger;
    private readonly IMongoCollection<Gem> _container;

    public GemRepository(MongoFactory factory, ILogger<GemRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Gem>("gems");
    }

    public async Task<List<Gem>> FetchAllGemsForCollection(string collectionId)
    {
        _logger.LogDebug("Entered FetchAllGemsForCollection function");
        _logger.LogInformation($"Collection id: {collectionId}");

        var filter = Builders<Gem>.Filter.Where(a => a.CollectionId == collectionId && a.Deleted == false && a.PublishedStatus == 1);

        return await _container.Find(filter).ToListAsync();
    }

    public async Task<GemsPage> GetGemsPageByCollectionId(string collectionId, int currentPage, int pageSize)
    {
        _logger.LogDebug("Entered GetGemsPageByCollectionId function");
        _logger.LogInformation($"Collection Id: {collectionId} | CurrentPage: {currentPage} | PageSize: {pageSize} | CollectionId: {collectionId}");

        try
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<Gem, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<Gem>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<Gem, Gem>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(Builders<Gem>.Sort.Ascending(x => x.CreatedOn)),
                PipelineStageDefinitionBuilder.Skip<Gem>(currentPage == 0 ? 0 : (currentPage * pageSize)),
                PipelineStageDefinitionBuilder.Limit<Gem>(pageSize),
                  }));

            var filter = Builders<Gem>.Filter.Where(x => x.CollectionId == collectionId && x.Deleted == false && x.PublishedStatus == 1);

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
                .Output<Gem>();

            return new GemsPage
            {
                Gems = new List<Gem>(data),
                ThisPage = currentPage + 1,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repo error during fetch of Gems for this Collection");
            return null;
        }
    }

    public async Task<Gem> FetchSingleGem(string gemId)
    {
        _logger.LogDebug("Entered FetchSingleGem function");
        _logger.LogInformation($"Gem Id: {gemId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == gemId && a.PublishedStatus == 1 && a.Deleted == false);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<Gem> FetchSingleGemAnyStatus(string gemId)
    {
        _logger.LogDebug("Entered FetchSingleGemAnyStatus function");
        _logger.LogInformation($"Gem Id: {gemId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == gemId && a.Deleted == false);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }
}