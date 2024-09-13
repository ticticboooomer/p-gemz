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
    public class CreatorToOpenRepository : ICreatorToOpenRepository
    {
        private readonly MongoFactory _factory;
        private readonly ILogger<CreatorToOpenRepository> _logger;
        private readonly IMongoCollection<CreatorToOpen> _container;

        public CreatorToOpenRepository(MongoFactory factory, ILogger<CreatorToOpenRepository> logger)
        {
            _factory = factory;
            _logger = logger;
            var db = _factory.GetDatabase();
            _container = db.GetCollection<CreatorToOpen>("creator_to_open");
        }

        public async Task<CreatorToOpen> CreateAsync(CreatorToOpen entity)
        {
            _logger.LogDebug("Entered CreateAsync function");
            _logger.LogInformation($"Creating CreatorToOpen with id: {entity.Id}");

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
}
