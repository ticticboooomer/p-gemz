using System.Net;
using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SharpCompress.Common;

namespace Gemz.Api.Creator.Data.Repository;

public class StoreTagRepository : IStoreTagRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<StoreTagRepository> _logger;
    private readonly IMongoCollection<StoreTag> _container;

    public StoreTagRepository(MongoFactory factory, ILogger<StoreTagRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<StoreTag>("store_tags");
    }

    public async Task<StoreTag> CreateAsync(StoreTag entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Creating StoreTag: {entity.Tagword}");

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

    public async Task<StoreTag> GetAsync(string tagWord)
    {
        _logger.LogDebug("Entered GetAsync function");
        _logger.LogInformation($"Parameter tagword: {tagWord}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Tagword == tagWord);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<bool> DeleteAsync(string tagWord)
    {
        _logger.LogDebug("Entered DeleteAsync in repo.");

        var filter = Builders<StoreTag>.Filter.Eq(s => s.Tagword, tagWord);

        var resp =  await _container.DeleteOneAsync(filter);

        return resp.IsAcknowledged;
    }
}