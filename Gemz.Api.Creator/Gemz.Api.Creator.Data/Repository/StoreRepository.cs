using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Creator.Data.Repository;

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
    
    public async Task<Store> GetByCreatorIdAsync(string creatorId)
    {
        _logger.LogDebug("Entered GetByCreatorIdAsync function");
        _logger.LogInformation($"Creator id: {creatorId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.CreatorId == creatorId);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<Store> UpdateAsync(Store entity)
    {
        _logger.LogDebug("Entered UpdateAsync function");
        _logger.LogInformation($"Store entity id: {entity.Id}");

        var update = Builders<Store>.Update.Set(a => a.Name, entity.Name)
            .Set(a => a.BannerImageId, entity.BannerImageId)
            .Set(a => a.LogoImageId, entity.LogoImageId)
            .Set(a => a.UrlStoreTag, entity.UrlStoreTag)
            .Set(a => a.MaxAutoOpenRarity, entity.MaxAutoOpenRarity);
        
        var resp = await _container.UpdateOneAsync(x => x.Id == entity.Id, update);

        _logger.LogDebug("Returned from ReplaceOneAsync call");
        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged ? entity : null;
    }

    public async Task<Store> CreateAsync(Store entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Creating Store with id: {entity.Id}");

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
}