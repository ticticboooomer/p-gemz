using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository;

public interface IStoreRepository
{
    Task<Store> GetByCreatorIdAsync(string creatorId);
    Task<Store> UpdateAsync(Store entity);

    Task<Store> CreateAsync(Store entity);
}