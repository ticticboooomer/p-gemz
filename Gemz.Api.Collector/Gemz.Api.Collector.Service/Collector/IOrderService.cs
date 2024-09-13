using Gemz.Api.Collector.Service.Collector.Model;
using Gemz.ServiceBus.Model;
using Stripe;

namespace Gemz.Api.Collector.Service.Collector;

public interface IOrderService
{
    Task<GenericResponse<OrderOutputModel>> FetchOrderById(string collectorId, OrderIdModel orderIdModel);

    Task<GenericResponse<OrderPisOutputModel>> FetchOrderByPaymentIntentSecret(string collectorId, PaymentIntentInputModel paymentIntentInputModel);

    Task<GenericResponse<OrderStatusOutputModel>> UpdateOrderStatus(string collectorId,
        OrderStatusInputModel orderStatusInputModel);

    Task<GenericResponse<PaymentPendingOutputModel>> UpdateOrderForPaymentPending(string collectorId, PaymentPendingInputModel paymentPendingInputModel);

    Task<GenericResponse<List<OrderHeaderOutputModel>>> FetchOrderList(string collectorId);

    Task UpdateOrderStatusFromStripeEvent(PaymentIntentMessageModel paymentIntentMessage);

    Task<GenericResponse<OrderListPagedOutputModel>> FetchOrderListPaged(string collectorId, OrderListPagedInputModel orderListPagedInputModel);
}