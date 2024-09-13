using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;

public interface IStoreTagRepository
{
    Task<StoreTag> GetAsync(string tagWord);

    Task<StoreTag> GetByCreatorIdAsync(string creatorId);
}