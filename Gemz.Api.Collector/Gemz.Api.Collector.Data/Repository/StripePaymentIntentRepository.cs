using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Gemz.Api.Collector.Data.Repository;

public class StripePaymentIntentRepository : IStripePaymentIntentRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<StripePaymentIntentRepository> _logger;
    private readonly IMongoCollection<StripePaymentIntent> _container;
    
    public StripePaymentIntentRepository(MongoFactory factory, ILogger<StripePaymentIntentRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<StripePaymentIntent>("stripe_payment_intents");
    }
    
    public async Task<StripePaymentIntent> CreateAsync(StripePaymentIntent entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Creating StripePaymentIntent with id: {entity.Id}");

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