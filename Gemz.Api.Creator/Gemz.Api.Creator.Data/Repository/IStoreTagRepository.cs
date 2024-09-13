using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository;

public interface IStoreTagRepository
{
    Task<StoreTag> CreateAsync(StoreTag entity);

    Task<StoreTag> GetAsync(string tagWord);

    Task<bool> DeleteAsync(string tagWord);
}