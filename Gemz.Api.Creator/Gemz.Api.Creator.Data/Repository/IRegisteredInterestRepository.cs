using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository
{
    public interface IRegisteredInterestRepository
    {
        Task<RegisteredInterest> CreateAsync(RegisteredInterest entity);
        Task<RegisteredInterest> GetByAccountIdAsync(string accountId);
    }
}
