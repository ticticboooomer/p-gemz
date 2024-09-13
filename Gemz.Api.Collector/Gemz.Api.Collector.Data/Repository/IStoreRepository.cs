using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;

public interface IStoreRepository
{
    Task<Store> GetByCreatorId(string creatorId);

    Task<List<Store>> GetAllLiveStores();

    Task<StoresPage> GetLiveStoresPage(int currentPage, int pageSize);
}