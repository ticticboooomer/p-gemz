using Gemz.Api.Collector.Service.Collector.Model;

namespace Gemz.Api.Collector.Service.Collector;

public interface IStoreService
{
    Task<GenericResponse<StoreValidityModel>> CheckStoreIsValid(StoreTagModel storeTagModel);
    Task<GenericResponse<StoreFrontModel>> FetchCreatorStoreFront(StoreTagModel storeTagModel);
    Task<GenericResponse<List<LiveStoresOutputModel>>> FetchLiveStoresList();
    Task<GenericResponse<LiveStoresPagedOutputModel>> FetchLiveStoresPage(LiveStoresPagedInputModel liveStoresPagedInputModel);
}