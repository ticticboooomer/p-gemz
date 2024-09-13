using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Creator.Data.Repository
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

        public async Task<List<CreatorToOpen>> GetAllForOneCreator(string creatorId)
        {
            _logger.LogDebug("Entered GetAllForOneCreator function in CreatorToOpen Repo");
            _logger.LogInformation($"creatorId: {creatorId}");

            var filter = Builders<CreatorToOpen>.Filter.Where(a => a.CreatorId == creatorId);

            return await _container.Find(filter).ToListAsync();
        }

        public async Task<CreatorToOpen> GetById(string id)
        {
            _logger.LogDebug("Entered GetById function");
            _logger.LogInformation($"CreatorToOpen Id: {id}");

            var filter = Builders<CreatorToOpen>.Filter.Where(x => x.Id == id);

            return await _container.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteSingle(string id)
        {
            _logger.LogDebug("Entered DeleteSingle in repo.");

            var filter = Builders<CreatorToOpen>.Filter.Eq(c => c.Id, id);

            var resp = await _container.DeleteOneAsync(filter);

            return resp.IsAcknowledged;
        }
    }
}
