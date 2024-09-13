using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;

public interface IOrderRepository
{
    Task<Order> FetchOrderByIdAsync(string orderId);
    Task<Order> FetchOrderByPaymentIntentSecretAsync(string paymentIntentSecret);

    Task<Order> CreateOrderAsync(Order entity);

    Task<bool> PatchOrderStatus(string orderId, OrderStatus orderStatus);
    Task<bool> PatchOrderStripeErrorMessage(string orderId, string stripeErrorMessage);

    Task<bool> PatchPaymentIntentClientSecret(string orderId, string paymentIntentClientSecret, string paymentConnectedStripeAccount);

    Task<List<Order>> FetchOrdersForCollector(string collectorId);
    Task<OrderPage> GetPageOfOrdersForCollector(string collectorId, int currentPage, int pageSize);
}