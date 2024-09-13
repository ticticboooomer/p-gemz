using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository
{
    public class CollectorPackRepository : ICollectorPackRepository
    {
        private readonly MongoFactory _factory;
        private readonly ILogger<CollectorPackRepository> _logger;
        private readonly IMongoCollection<CollectorPack> _container;

        public CollectorPackRepository(MongoFactory factory, ILogger<CollectorPackRepository> logger)
        {
            _factory = factory;
            _logger = logger;
            var db = _factory.GetDatabase();
            _container = db.GetCollection<CollectorPack>("collector_packs");
        }

        public async Task<CollectorPack> CreateAsync(CollectorPack entity)
        {
            _logger.LogDebug("Entered CreateAsync function");
            _logger.LogInformation($"Creating CollectorPack with id: {entity.Id}");

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

        public async Task<List<CollectorPack>> FetchUnopenedPacksForCollector(string collectorId)
        {
            _logger.LogDebug("Entered FetchUnopenedPacksForCollector function.");

            var filter = Builders<CollectorPack>.Filter.Where(a => a.CollectorId == collectorId);

            return await _container.Find(filter).ToListAsync();
        }

        public async Task<List<CollectorPack>> FetchUnopenedPacksInCollection(string collectorId, string collectionId)
        {
            _logger.LogDebug("Entered FetchUnopenedPacksInCollection function.");

            var filter = Builders<CollectorPack>.Filter.Where(a => a.CollectorId == collectorId && a.CollectionId == collectionId);

            return await _container.Find(filter).ToListAsync();
        }

        public async Task<bool> DeleteAsync(string collectorPackId)
        {
            _logger.LogDebug("Entered DeleteAsync in repo.");

            var filter = Builders<CollectorPack>.Filter.Eq(c => c.Id, collectorPackId);

            var resp = await _container.DeleteOneAsync(filter);

            return resp.IsAcknowledged;
        }

        public async Task<List<CollectorPack>> GetAllAsync()
        {
            _logger.LogDebug("Entered FetchAllPacks function.");

            var filter = Builders<CollectorPack>.Filter.Empty;

            return await _container.Find(filter).ToListAsync();
        }

        public async Task<CollectorPack> FetchPackForOrderLineAsync(string orderId, string orderLineId)
        {
            _logger.LogDebug("Entered FetchPackForOrderLineAsync function");
            _logger.LogInformation($"OrderId: {orderId}  |  orderLineId: {orderLineId}");

            var response = await _container.AsQueryable()
                .FirstOrDefaultAsync(p => p.OriginatingOrderId == orderId && p.OriginatingOrderLineId == orderLineId);

            _logger.LogDebug("Returned from FirstOrDefaultAsync call");

            if (response is null)
            {
                _logger.LogWarning("FirstOrDefaultAsync did not return a collector Pack");
            }

            return response;
        }
    }
}
