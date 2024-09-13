using Gemz.Api.Auth.Service.Accounts.Model;
using Gemz.Api.Auth.Service.Auth.Model;

namespace Gemz.Api.Auth.Service.Accounts;

public interface IAccountService
{
    Task<GenericResponse<AccountModel>> GetAccountById(string accountId);
    Task<GenericResponse<AccountModel>> UpdateAccount(string accountId, AccountUpdateModel accountUpdateModel);
}