using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository
{
    public class OpenPackSessionRepository : IOpenPackSessionRepository
    {
        private readonly MongoFactory _factory;
        private readonly ILogger<OpenPackSessionRepository> _logger;
        private readonly IMongoCollection<OpenPackSession> _container;

        public OpenPackSessionRepository(MongoFactory factory, ILogger<OpenPackSessionRepository> logger)
        {
            _factory = factory;
            _logger = logger;
            var db = _factory.GetDatabase();
            _container = db.GetCollection<OpenPackSession>("open-pack-sessions");
        }

        public async Task<OpenPackSession> GetOpenPackSessionAsync(string sessionId)
        {
            _logger.LogDebug("Entered GetOpenPackSessionAsync function");
            _logger.LogInformation($"SessionId: {sessionId}");

            var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == sessionId);

            _logger.LogDebug("Returned from FirstOrDefaultAsync call");

            if (response is null)
            {
                _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
            }

            return response;
        }

        public async Task<OpenPackSession> CreateOpenPackSessionAsync(OpenPackSession openPackSession)
        {
            _logger.LogDebug("Entered CreateOpenPackSessionAsync function");
            _logger.LogInformation($"Creating OpenPackSession with id: {openPackSession.Id}");

            try
            {
                await _container.InsertOneAsync(openPackSession);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "InsertOneAsync threw an exception");
                return null;
            }
            _logger.LogInformation($"InsertOneAsync succeeded");

            return openPackSession;
        }
    }
}
