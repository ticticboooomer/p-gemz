using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository;

public class StoreRepository : IStoreRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<StoreRepository> _logger;
    private readonly IMongoCollection<Store> _container;

    public StoreRepository(MongoFactory factory, ILogger<StoreRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Store>("stores");
    }

    public async Task<Store> GetByCreatorId(string creatorId)
    {
        _logger.LogDebug("Entered GetByCreatorId function");
        _logger.LogInformation($"Creator id: {creatorId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.CreatorId == creatorId);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<List<Store>> GetAllLiveStores()
    {
        _logger.LogDebug("Entered GetAllLiveStores function.");

        var filter = Builders<Store>.Filter.Where(a => a.LiveDate <= DateTime.UtcNow);

        return await _container.Find(filter).ToListAsync();
    }

    public async Task<StoresPage> GetLiveStoresPage(int currentPage, int pageSize)
    {
        _logger.LogDebug("Entered GetLiveStoresPage function");
        _logger.LogInformation($"PageSize: {pageSize} | CurrentPage: {currentPage}");

        try
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<Store, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<Store>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<Store, Store>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(Builders<Store>.Sort.Ascending(x => x.CreatedOn)),
                PipelineStageDefinitionBuilder.Skip<Store>(currentPage == 0 ? 0 : (currentPage * pageSize)),
                PipelineStageDefinitionBuilder.Limit<Store>(pageSize)

                }));

            var filter = Builders<Store>.Filter.Where( s => s.LiveDate <= DateTime.UtcNow);

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
                .Output<Store>();

            return new StoresPage()
            {
                Stores = new List<Store>(data),
                ThisPage = currentPage + 1,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repo error during fetch of Live Stores Page");
            return null;
        }
    }
}