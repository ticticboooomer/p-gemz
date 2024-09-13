using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository;

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

    public async Task<StoreTag> GetAsync(string tagWord)
    {
        _logger.LogDebug("Entered GetAsync function");
        _logger.LogInformation($"Parameter tagword: {tagWord}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.Tagword == tagWord);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a StoreTag");
        }

        return response;
    }

    public async Task<StoreTag> GetByCreatorIdAsync(string creatorId)
    {
        _logger.LogDebug("Entered GetByCreatorIdAsync function");
        _logger.LogInformation($"Parameter creatorId: {creatorId}");


        var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.CreatorId == creatorId);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a StoreTag");
        }

        return response;
    }
}