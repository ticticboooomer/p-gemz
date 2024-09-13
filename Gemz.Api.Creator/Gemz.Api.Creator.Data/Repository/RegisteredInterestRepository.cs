using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Creator.Data.Repository
{
    public class RegisteredInterestRepository : IRegisteredInterestRepository
    {
        private readonly MongoFactory _factory;
        private readonly ILogger<RegisteredInterestRepository> _logger;
        private readonly IMongoCollection<RegisteredInterest> _container;

        public RegisteredInterestRepository(MongoFactory factory, ILogger<RegisteredInterestRepository> logger)
        {
            this._factory = factory;
            _logger = logger;
            var db = _factory.GetDatabase();
            _container = db.GetCollection<RegisteredInterest>("registered_interest");
        }

        public async Task<RegisteredInterest> CreateAsync(RegisteredInterest entity)
        {
            _logger.LogDebug("Entered CreateAsync function");
            _logger.LogInformation($"Creating RegisteredInterest with id: {entity.Id}");

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

        public async Task<RegisteredInterest> GetByAccountIdAsync(string accountId)
        {
            _logger.LogDebug("Entered GetByAccountIdAsync function");
            _logger.LogInformation($"accountId: {accountId}");

            var response = await _container.AsQueryable()
                .FirstOrDefaultAsync(x => x.AccountId == accountId);

            _logger.LogDebug("Returned from FirstOrDefaultAsync call");

            if (response is null)
            {
                _logger.LogWarning("FirstOrDefaultAsync did not return a registered_interest");
            }

            return response;

        }
    }
}
