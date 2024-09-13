using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SharpCompress.Common;
using System;

namespace Gemz.Api.Creator.Data.Repository
{
    public class CollectorGemRepository : ICollectorGemRepository
    {
        private readonly MongoFactory _factory;
        private readonly ILogger<CollectorGemRepository> _logger;
        private readonly IMongoCollection<CollectorGem> _container;

        public CollectorGemRepository(MongoFactory factory, ILogger<CollectorGemRepository> logger)
        {
            _factory = factory;
            _logger = logger;
            var db = _factory.GetDatabase();
            _container = db.GetCollection<CollectorGem>("collector_gems");
        }

        public async Task<CollectorGem> GetFirstForCollection(string creatorId, string collectionId)
        {
            _logger.LogDebug("Entered GetFirstForCollection function");
            _logger.LogInformation($"Creator Id: {creatorId} | Collection Id: {collectionId}");

            var filter = Builders<CollectorGem>.Filter.Where(x => x.CreatorId == creatorId && x.CollectionId == collectionId);

            return await _container.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<CollectorGem> GetFirstForGem(string creatorId, string gemId)
        {
            _logger.LogDebug("Entered GetFirstForGem function");
            _logger.LogInformation($"Creator Id: {creatorId} | Gem Id: {gemId}");

            var filter = Builders<CollectorGem>.Filter.Where(x => x.CreatorId == creatorId && x.GemId == gemId);

            return await _container.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<CollectorGem> GetSingleCollectorGem(string collectorGemId)
        {
            _logger.LogDebug("Entered GetSingleCollectorGem function");
            _logger.LogInformation($"Collector Gem Id: {collectorGemId}");

            var filter = Builders<CollectorGem>.Filter.Where(x => x.Id == collectorGemId);

            return await _container.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<CollectorGem> UpdateSingle(CollectorGem entity)
        {
            _logger.LogDebug("Entered UpdateSingle in repo.");
            _logger.LogInformation($"CollectorGem entity Id: {entity.Id}");

            var update = Builders<CollectorGem>.Update.Set(a => a.Visible, entity.Visible);
            
            var resp = await _container.UpdateOneAsync(x => x.Id == entity.Id, update);

            _logger.LogDebug("Returned from ReplaceOneAsync call");
            _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

            return resp.IsAcknowledged ? entity : null;
        }
    }
}
