using System.Net;
using System.Security.Cryptography.X509Certificates;
using Gemz.Api.Auth.Data.Factory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Auth.Data.Repository.Account;

public class AccountRepository : IAccountRepository
{
    private readonly DbFactory _factory;
    private readonly ILogger<AccountRepository> _logger;
    private readonly IMongoCollection<Model.Account> _container;

    public AccountRepository(DbFactory factory, ILogger<AccountRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Model.Account>("accounts");
    }

    public async Task<Model.Account> CreateAsync(Model.Account entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Creating account with id: {entity.Id}");
        try
        {
            await _container.InsertOneAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InsertOneAsync threw an exception");
            return null;
        }

        _logger.LogInformation($"InsertOneAsync succeeded");

        return entity;
    }

    public async Task<Model.Account> GetAsync(string id)
    {
        _logger.LogDebug("Entered GetAsync function");
        _logger.LogInformation($"Parameter id: {id}");

        var response = await _container.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        _logger.LogDebug("Returned from FirstOrDefaultAsync call");
        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return an account");
        }

        return response;
    }

    public async Task<Model.Account> GetByTwitchUserIdAsync(string twitchUserId)
    {
        _logger.LogDebug("Entered GetByTwitchUserIdAsync function");

        return await _container.AsQueryable()
            .FirstOrDefaultAsync(a => a.TwitchUserId == twitchUserId);
    }

    public async Task<bool> PatchEmailAddress(string accountId, string emailAddress)
    {
        _logger.LogDebug("Entered PatchEmailAddress in repo.");


        var filter = Builders<Model.Account>.Filter.Eq(c => c.Id, accountId);

        var update = Builders<Model.Account>.Update.Set(c => c.EmailAddress, emailAddress);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged;
    }

    public async Task<bool> PatchTwitchTokens(string accountId, string twitchAccessCode, string twitchRefreshCode)
    {
        {
            _logger.LogDebug("Entered PatchTwitchTokens in repo.");


            var filter = Builders<Model.Account>.Filter.Eq(a => a.Id, accountId);

            var update = Builders<Model.Account>.Update.Set(a => a.Tokens.AccessCode, twitchAccessCode)
                .Set(a => a.Tokens.RefreshCode, twitchRefreshCode);

            var resp = await _container.UpdateOneAsync(filter, update);

            _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

            return resp.IsAcknowledged;
        }
    }

    public async Task<bool> PatchTwitchData(string accountId, string twitchAccessCode, string twitchRefreshCode,
        string twitchEmail,
        bool twitchEmailVerified, string twitchUsername)
    {
        _logger.LogDebug("Entered PatchTwitchData in repo.");


        var filter = Builders<Model.Account>.Filter.Eq(a => a.Id, accountId);

        var update = Builders<Model.Account>.Update.Set(a => a.Tokens.AccessCode, twitchAccessCode)
            .Set(a => a.Tokens.RefreshCode, twitchRefreshCode)
            .Set(a => a.TwitchEmail, twitchEmail)
            .Set(a => a.TwitchEmailVerified, twitchEmailVerified)
            .Set(a => a.TwitchUsername, twitchUsername);

        var resp = await _container.UpdateOneAsync(filter, update);

        _logger.LogInformation($"UpdateOneAsync returned ack of: {resp.IsAcknowledged}");

        return resp.IsAcknowledged;
    }
}