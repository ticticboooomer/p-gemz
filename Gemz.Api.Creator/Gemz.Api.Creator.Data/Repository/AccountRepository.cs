using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Creator.Data.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly MongoFactory _factory;
        private readonly ILogger<AccountRepository> _logger;
        private readonly IMongoCollection<Account> _container;

        public AccountRepository(MongoFactory factory, ILogger<AccountRepository> logger)
        {
            _factory = factory;
            _logger = logger;
            var db = _factory.GetDatabase();
            _container = db.GetCollection<Model.Account>("accounts");
        }
        public async Task<Account> GetAsync(string id)
        {
            _logger.LogDebug("Entered GetAsync function in AccountRepository");
            _logger.LogInformation($"Parameter id: {id}");

            var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
            _logger.LogDebug("Returned from FirstOrDefaultAsync call");
            if (response is null)
            {
                _logger.LogWarning("FirstOrDefaultAsync did not return an account");
            }
            return response;
        }


        public async Task<bool> PatchOnboardingFields(string creatorId, int onboardingStatus, 
                                                        string stripeAccountId,
                                                        bool detailsSubmitted,
                                                        bool chargesEnabled)
        {
            _logger.LogDebug("Entered PatchOnboardingFields in repo.");

            var filter = Builders<Account>.Filter.Eq(c => c.Id, creatorId);

            var update = Builders<Account>.Update.Set(c => c.OnboardingStatus, onboardingStatus)
                                                .Set(c => c.StripeAccountId, stripeAccountId)
                                                .Set(c => c.StripeDetailsSubmitted, detailsSubmitted)
                                                .Set(c => c.StripeChargesEnabled, chargesEnabled);

            var resp = await _container.UpdateOneAsync(filter, update);

            _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

            return resp.IsAcknowledged;
        }

        public async Task<bool> PatchStripeAccountSettings(string creatorId, 
                                                        int onboardingStatus, 
                                                        bool detailsSubmitted, 
                                                        bool chargesEnabled,
                                                        int? commissionPercentage)
        {
            _logger.LogDebug("Entered PatchStripeAccountSettings in repo.");

            var filter = Builders<Account>.Filter.Eq(c => c.Id, creatorId);

            var update = Builders<Account>.Update.Set(c => c.OnboardingStatus, onboardingStatus)
                .Set(c => c.StripeDetailsSubmitted, detailsSubmitted)
                .Set(c => c.StripeChargesEnabled, chargesEnabled)
                .Set(c => c.CommissionPercentage, commissionPercentage);

            var resp = await _container.UpdateOneAsync(filter, update);

            _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

            return resp.IsAcknowledged;
        }
    }
}
