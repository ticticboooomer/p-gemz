namespace Gemz.Api.Auth.Data.Repository.Account;

public interface IAccountRepository
{
    Task<Model.Account> CreateAsync(Model.Account entity);
    
    Task<Model.Account> GetAsync(string id);

    Task<Model.Account> GetByTwitchUserIdAsync(string twitchUserId);

    Task<bool> PatchEmailAddress(string accountId, string emailAddress);
    Task<bool> PatchTwitchTokens(string accountId, string twitchAccessCode, string twitchRefreshCode);

    Task<bool> PatchTwitchData(string accountId, string twitchAccessCode, string twitchRefreshCode, string twitchEmail,
        bool twitchEmailVerified, string twitchUsername);
}
