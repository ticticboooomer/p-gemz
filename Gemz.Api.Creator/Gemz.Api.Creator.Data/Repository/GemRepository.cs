using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;

namespace Gemz.Api.Creator.Data.Repository;

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

    public async Task<Gem> GetAsync(string gemId)
    {
        _logger.LogDebug("Entered GetAsync function");
        _logger.LogInformation($"Gem id: {gemId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.Id == gemId && x.Deleted == false);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a gem");
        }

        return response;
    }


    public async Task<bool> PatchItemPublishedStatusAsync(string gemId, int publishedStatus)
    {
        _logger.LogDebug("Entered PatchItemPublishedStatusAsync in repo.");

        var filter = Builders<Gem>.Filter.Eq(c => c.Id, gemId);

        var update = Builders<Gem>.Update.Set(c => c.PublishedStatus, publishedStatus);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? true : false;
    }

    public async Task<Gem> UpdateGemAsync(Gem entity)
    {
        _logger.LogDebug("Entered UpdateGemAsync in repo.");
        _logger.LogInformation($"Gem entity Id: {entity.Id}");

        var update = Builders<Gem>.Update.Set(a => a.Deleted, entity.Deleted)
            .Set(a => a.Name, entity.Name)
            .Set(a => a.PublishedStatus, entity.PublishedStatus)
            .Set(a => a.Rarity, entity.Rarity)
            .Set(a => a.ImageId, entity.ImageId)
            .Set(a => a.SizePercentage, entity.SizePercentage)
            .Set(a => a.PositionXPercentage, entity.PositionXPercentage)
            .Set(a => a.PositionYPercentage, entity.PositionYPercentage);
        
        var resp = await _container.UpdateOneAsync(x => x.Id == entity.Id, update);

        _logger.LogDebug("Returned from ReplaceOneAsync call");
        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? entity : null;
    }


    public async Task<Gem> CreateAsync(Gem entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Creating Gem with id: {entity.Id}");

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

    public async Task<bool> PatchGemDeletedAsync(string gemId)
    {
        _logger.LogDebug("Entered PatchGemDeletedAsync in repo.");

        var filter = Builders<Gem>.Filter.Eq(c => c.Id, gemId);

        var update = Builders<Gem>.Update.Set(c => c.Deleted, true);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? true : false;
    }

    public async Task<GemsPage> GetGemsPageByCollectionId(string collectionId, string creatorId,
        int currentPage, int pageSize)
    {
        _logger.LogDebug("Entered GetGemsPageByCollectionId function");
        _logger.LogInformation($"Creator Id: {creatorId} | PageSize: {pageSize} | CollectionId: {collectionId}");

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
                PipelineStageDefinitionBuilder.Sort(Builders<Gem>.Sort.Descending(x => x.Rarity)),
                PipelineStageDefinitionBuilder.Skip<Gem>(currentPage == 0 ? 0 : (currentPage * pageSize)),
                PipelineStageDefinitionBuilder.Limit<Gem>(pageSize),
                  }));

            var filter = Builders<Gem>.Filter.Where(x => x.CollectionId == collectionId && x.CreatorId == creatorId && x.Deleted == false);

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
            _logger.LogWarning(ex, "Repo error during fetch of Gems for this CreatorId / Collection combo");
            return null;
        }
    }

    public async Task<bool> AnyPublishedGemsInCollection(string collectionId)
    {
        var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.CollectionId == collectionId && x.Deleted == false && x.PublishedStatus == 1);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a gem");
            return false;
        }

        return true;

    }
}