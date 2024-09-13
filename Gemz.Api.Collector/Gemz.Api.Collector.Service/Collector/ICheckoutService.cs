using Gemz.Api.Collector.Service.Collector.Model;

namespace Gemz.Api.Collector.Service.Collector;

public interface ICheckoutService
{
    Task<GenericResponse<StripePaymentInformation>> CreatePaymentIntent(string collectorId, CreatePIInputModel createPIInputModel);
    Task<GenericResponse<OrderFromBasketOutputModel>> CreateNewOrderFromActiveBasket(string collectorId, OrderFromBasketInputModel orderFromBasketInputModel);

    Task<GenericResponse<OrderFromFailOutputModel>> CreateNewOrderFromFailedOrder(string collectorId, OrderFromFailInputModel orderFromFailInputModel);
}