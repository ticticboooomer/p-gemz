using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository;

public class BasketRepository : IBasketRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<BasketRepository> _logger;
    private readonly IMongoCollection<Basket> _container;
    
    public BasketRepository(MongoFactory factory, ILogger<BasketRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Basket>("baskets");
    }


    public async Task<Basket> GetActiveBasketById(string basketId)
    {
        _logger.LogDebug("Entered GetActiveBasketById function.");
        _logger.LogInformation($"basketId: {basketId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == basketId
                                                                               && a.Active == true);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        return response;
    }

    public async Task<Basket> GetCreatorBasketForCollector(string collectorId, string creatorId)
    {
        _logger.LogDebug("Entered GetBasketForCollector function.");
        _logger.LogInformation($"CollectorId: {collectorId}");
        _logger.LogInformation($"CreatorId: {creatorId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.CollectorId == collectorId 
                                                                               && a.Active == true
                                                                               && a.CreatorId == creatorId);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        return response;
    }

    public async Task<List<Basket>> GetAllBasketsForCollector(string collectorId)
    {
        _logger.LogDebug("Entered GetAllBasketsForCollector function");
        _logger.LogInformation($"collectorId: {collectorId}");

        var filter = Builders<Basket>.Filter.Where(x => x.CollectorId == collectorId && x.Active == true);

        return await _container.Find(filter).ToListAsync();
    }

    public async Task<Basket> UpdateBasketAsync(Basket entity)
    {
        _logger.LogDebug("Entered UpdateBasketAsync function");
        _logger.LogInformation($"Basket Id: {entity.Id} | CollectorId: {entity.CollectorId}");

         var update = Builders<Basket>.Update.Set(a => a.Active, entity.Active)
            .Set(a => a.Items, entity.Items);
        
        var resp = await _container.UpdateOneAsync(x => x.Id == entity.Id, update);

        _logger.LogDebug("Returned from ReplaceOneAsync call");
        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? entity : null;
    }

    public async Task<Basket> CreateBasketAsync(Basket entity)
    {
        _logger.LogDebug("Entered CreateBasketAsync function");
        _logger.LogInformation($"Creating Basket with id: {entity.Id}");

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

    public async Task<bool> DeactivateBasket(string basketId)
    {
        _logger.LogDebug("Entered UpdateBasketActiveStatus in repo.");

        var filter = Builders<Basket>.Filter.Eq(c => c.Id, basketId);

        var update = Builders<Basket>.Update.Set(c => c.Active, false);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged;
    }

    public async Task<BasketsPage> GetPageOfBasketsForCollector(string collectorId, int currentPage, int pageSize,
        string excludeCreator)
    {
        _logger.LogDebug("Entered GetPageOfBasketsForCollector function");
        _logger.LogInformation($"CollectorId: {collectorId} | PageSize: {pageSize} | CurrentPage: {currentPage} | excludeCreator: {excludeCreator}");

        try
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<Basket, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<Basket>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<Basket, Basket>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(Builders<Basket>.Sort.Descending(x => x.CreatedOn)),
                PipelineStageDefinitionBuilder.Skip<Basket>(currentPage == 0 ? 0 : (currentPage * pageSize)),
                PipelineStageDefinitionBuilder.Limit<Basket>(pageSize)

                }));

            var filter = Builders<Basket>.Filter.Where(x => x.CollectorId == collectorId 
                                                            && x.Active == true 
                                                            && x.CreatorId != excludeCreator);

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
                .Output<Basket>();

            return new BasketsPage()
            {
                Baskets = new List<Basket>(data),
                ThisPage = currentPage + 1,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repo error during fetch of Baskets for this CollectorId");
            return null;
        }
    }

    public async Task<Basket> GetAnyStatusBasketById(string basketId)
    {
        _logger.LogDebug("Entered GetAnyStatusBasketById function.");
        _logger.LogInformation($"basketId: {basketId}");

        var response = await _container.AsQueryable()
            .FirstOrDefaultAsync(a => a.Id == basketId);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        return response;
    }

}