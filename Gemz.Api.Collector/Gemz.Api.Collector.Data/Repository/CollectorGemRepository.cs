using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Repository
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

        public async Task<CollectorGem> CreateAsync(CollectorGem entity)
        {
            _logger.LogDebug("Entered CreateAsync function");
            _logger.LogInformation($"Creating CollectorGem with id: {entity.Id}");

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

        public async Task<List<CollectorGem>> GetForSingleCollectorAsync(string collectorId)
        {
            _logger.LogDebug("Entered GetForSingleCollectorAsync function");
            _logger.LogInformation($"Collector Id: {collectorId}");

            var filter = Builders<CollectorGem>.Filter.Where(x => x.CollectorId == collectorId);

            return await _container.Find(filter).ToListAsync();
        }

        public async Task<List<CollectorGem>> GetForSingleCreatorAndCollector(string collectorId, string creatorId)
        {
            _logger.LogDebug("Entered GetForSingleCreatorAndCollector function");
            _logger.LogInformation($"Collector Id: {collectorId} | CreatorId: {creatorId}");

            var filter = Builders<CollectorGem>.Filter.Where(x => x.CollectorId == collectorId && x.CreatorId == creatorId);

            return await _container.Find(filter).ToListAsync();
        }

        public async Task<List<CollectorGem>> GetForSingleCollectorAndCollection(string collectorId, string collectionId)
        {
            _logger.LogDebug("Entered GetForSingleCollectorAndCollection function");
            _logger.LogInformation($"Collector Id: {collectorId} | CollectionId: {collectionId}");

            var filter = Builders<CollectorGem>.Filter.Where(x => x.CollectorId == collectorId && x.CollectionId == collectionId);

            return await _container.Find(filter).ToListAsync();
        }
    }
}
