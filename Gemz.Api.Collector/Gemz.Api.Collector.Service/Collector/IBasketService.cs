using Gemz.Api.Collector.Service.Collector.Model;

namespace Gemz.Api.Collector.Service.Collector;

public interface IBasketService
{
    Task<GenericResponse<BasketModel>> GetActiveBasketForCollectorById(string collectorId, BasketGetByIdInputModel basketGetByIdInputModel);
    Task<GenericResponse<BasketModel>> GetActiveBasketForCollectorStoreTag(string collectorId, BasketGetByStoreTagInputModel basketGetByStoreTagInputModel);

    Task<GenericResponse<BasketModel>> AddItemToBasket(string collectorId, BasketItemAddModel basketItemAddModel);

    Task<GenericResponse<BasketModel>> RemoveItemFromBasket(string collectorId, BasketItemRemoveModel basketItemModel);

    Task<GenericResponse<BasketModel>> UpdateItemInBasket(string collectorId, BasketItemUpdateModel basketItemModel);

    Task<GenericResponse<BasketModel>> EmptyBasket(string collectorId, EmptyBasketInputModel emptyBasketInputModel);

    Task<GenericResponse<List<BasketIdOutputModel>>> GetAllActiveBasketsForOneCollector(string collectorId);
    Task<GenericResponse<BasketItemOutputModel>> GetBasketItemData(BasketItemInputModel basketItemInputModel);

    Task<GenericResponse<BasketPagedOutputModel>> GetAllActiveBasketsForOneCollectorPaged(string collectorId, GetAllBasketsPageInputModel getAllBasketsPageInputModel);

    Task<GenericResponse<StripeAccountOutputModel>> GetPaymentAccountByBasketId(
        PaymentAccountInputModel paymentAccountInputModel);
}