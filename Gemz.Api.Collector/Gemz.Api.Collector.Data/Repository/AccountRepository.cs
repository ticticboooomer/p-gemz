using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;

namespace Gemz.Api.Collector.Data.Repository;
public class AccountRepository : IAccountRepository
{
    private readonly MongoFactory _factory;
    private readonly ILogger<AccountRepository> _logger;
    private readonly IMongoCollection<Account> _container;

    public AccountRepository(MongoFactory factory, ILogger<AccountRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = factory.GetDatabase();
        _container = db.GetCollection<Account>("accounts");
    }

    public async Task<Account> GetAccountById(string id)
    {
        _logger.LogDebug("Entered GetAccountById function");
        _logger.LogInformation($"Account Id: {id}");
        var response = await _container.AsQueryable().FirstOrDefaultAsync(a => a.Id == id);

        _logger.LogDebug("Returned from FirstOrDefaultAsync call");
        if (response is null)
        {
            _logger.LogWarning("FirstOrDefaultAsync did not return an account");
        }

        return response;
    }
}
