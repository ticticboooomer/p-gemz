using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Collector.Data.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<OrderRepository> _logger;
    private readonly IMongoCollection<Order> _container;
    
    public OrderRepository(MongoFactory factory, ILogger<OrderRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Order>("orders");
    }
    
    public async Task<Order> FetchOrderByIdAsync(string orderId)
    {
        _logger.LogDebug("Entered FetchOrderByIdAsync function");
        _logger.LogInformation($"Order id: {orderId}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == orderId);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<Order> FetchOrderByPaymentIntentSecretAsync(string paymentIntentSecret)
    {
        _logger.LogDebug("Entered FetchOrderByPaymentIntentSecretAsync function");
        _logger.LogInformation($"Payment Intent Secret: {paymentIntentSecret}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.PaymentIntentClientSecret == paymentIntentSecret);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");

        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return a collection");
        }

        return response;
    }

    public async Task<Order> CreateOrderAsync(Order entity)
    {
        _logger.LogDebug("Entered CreateOrderAsync function");
        _logger.LogInformation($"Creating Order with id: {entity.Id}");

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

    public async Task<bool> PatchOrderStatus(string orderId, OrderStatus orderStatus)
    {
        _logger.LogDebug("Entered PatchOrderStatus in repo.");

        var filter = Builders<Order>.Filter.Eq(c => c.Id, orderId);

        var update = Builders<Order>.Update.Set(c => c.Status, orderStatus);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged;
    }

    public async Task<bool> PatchOrderStripeErrorMessage(string orderId, string stripeErrorMessage)
    {
        _logger.LogDebug("Entered PatchOrderStripeErrorMessage in repo.");

        var filter = Builders<Order>.Filter.Eq(c => c.Id, orderId);

        var update = Builders<Order>.Update.Set(c => c.StripeErrorMessage, stripeErrorMessage);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged;
    }

    public async Task<bool> PatchPaymentIntentClientSecret(string orderId, string paymentIntentClientSecret, string paymentConnectedStripeAccount)
    {
        _logger.LogDebug("Entered PatchPaymentIntentClientSecret in repo.");


        var filter = Builders<Order>.Filter.Eq(c => c.Id, orderId);

        var update = Builders<Order>.Update.Set(c => c.PaymentIntentClientSecret, paymentIntentClientSecret)
            .Set(o => o.PaymentConnectedStripeAccount, paymentConnectedStripeAccount);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged;
    }

    public async Task<List<Order>> FetchOrdersForCollector(string collectorId)
    {
        _logger.LogDebug("Entered FetchOrderForCollector function");
        _logger.LogInformation($"Collector Id: {collectorId}");

        var filter = Builders<Order>.Filter
            .Where(x => x.CollectorId == collectorId && x.Status != OrderStatus.New && x.Status != OrderStatus.Replaced);

        return await _container.Find(filter).ToListAsync();

    }

    public async Task<OrderPage> GetPageOfOrdersForCollector(string collectorId, int currentPage, int pageSize)
    {
        _logger.LogDebug("Entered GetPageOfOrdersForCollector function");
        _logger.LogInformation($"CollectorId: {collectorId} | PageSize: {pageSize} | CurrentPage: {currentPage}");

        try
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<Order, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<Order>()
                }));

            var dataFacet = AggregateFacet.Create("data",
                PipelineDefinition<Order, Order>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Sort(Builders<Order>.Sort.Descending(x => x.CreatedOn)),
                PipelineStageDefinitionBuilder.Skip<Order>(currentPage == 0 ? 0 : (currentPage * pageSize)),
                PipelineStageDefinitionBuilder.Limit<Order>(pageSize)

                }));

            var filter = Builders<Order>.Filter.Where(x => x.CollectorId == collectorId && 
                                                           x.Status != OrderStatus.New && 
                                                           x.Status != OrderStatus.Replaced);

            var aggregation = await _container.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count ?? 0;

            var totalPages = (int)count / pageSize;
            var remainder = (int)count % pageSize;
            if (remainder > 0) totalPages++;

            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<Order>();

            return new OrderPage()
            {
                Orders = new List<Order>(data),
                ThisPage = currentPage + 1,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repo error during fetch of Orders for this CollectorId");
            return null;
        }

    }
}