using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Repository
{
    public class CollectorPackOpenedRepository : ICollectorPackOpenedRepository
    {
        private readonly MongoFactory _factory;
        private readonly ILogger<CollectorPackOpenedRepository> _logger;
        private readonly IMongoCollection<CollectorPackOpened> _container;

        public CollectorPackOpenedRepository(MongoFactory factory, ILogger<CollectorPackOpenedRepository> logger)
        {
            _factory = factory;
            _logger = logger;
            var db = _factory.GetDatabase();
            _container = db.GetCollection<CollectorPackOpened>("collector_packs_opened");
        }
        public async Task<CollectorPackOpened> CreateAsync(CollectorPackOpened entity)
        {
            _logger.LogDebug("Entered CreateAsync function");
            _logger.LogInformation($"Creating CollectorPackOpened with id: {entity.Id}");

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

        public async Task<List<CollectorPackOpened>> GetAllAsync()
        {
            _logger.LogDebug("Entered GetAllAsync function.");

            var filter = Builders<CollectorPackOpened>.Filter.Empty;

            return await _container.Find(filter).ToListAsync();

        }

        public async Task<bool> ReplaceAsync(CollectorPackOpened entity)
        {
            _logger.LogDebug("Entered ReplaceAsync in repo.");

            var filter = Builders<CollectorPackOpened>.Filter.Eq(c => c.Id, entity.Id);
            var update = Builders<CollectorPackOpened>.Update.Set(a => a.OriginatingOrderId, entity.OriginatingOrderId)
                .Set(a => a.GemsCreatedFromPack, entity.GemsCreatedFromPack);

            var resp = await _container.ReplaceOneAsync(filter, entity);

            return resp.IsAcknowledged;
        }

        public async Task<CollectorPackOpened> FetchOpenedPackForOrderLineAsync(string orderId, string orderLineId)
        {
            _logger.LogDebug("Entered FetchOpenedPackForOrderLineAsync function");
            _logger.LogInformation($"OrderId: {orderId}  |  orderLineId: {orderLineId}");

            var response = await _container.AsQueryable()
                .FirstOrDefaultAsync(p => p.OriginatingOrderId == orderId && p.OriginatingOrderLineId == orderLineId);

            _logger.LogDebug("Returned from FirstOrDefaultAsync call");

            if (response is null)
            {
                _logger.LogWarning("FirstOrDefaultAsync did not return an opened collector Pack");
            }

            return response;
        }

    }
}
