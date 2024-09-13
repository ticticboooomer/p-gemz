using System.Net;
using Gemz.Api.Auth.Data.Factory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Gemz.Api.Auth.Data.Repository.AuthState;

public class AuthStateRepository : IAuthStateRepository
{
    private readonly DbFactory _factory;
    private readonly ILogger<AuthStateRepository> _logger;
    private readonly IMongoCollection<Model.AuthState> _container;

    public AuthStateRepository(DbFactory factory, ILogger<AuthStateRepository> logger)
    {
        _factory = factory;
        _logger = logger;
        var db = _factory.GetDatabase();
        _container = db.GetCollection<Model.AuthState>("auth_keys");
    }
    
    public async Task<Model.AuthState> CreateAsync(Model.AuthState entity)
    {
        _logger.LogDebug("Entered CreateAsync function");
        _logger.LogInformation($"Passed in entity with Id: {entity.Id}");
        try
        {
            await _container.InsertOneAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InsertOneAsync threw an exception");
            return null;
        }
        _logger.LogDebug("Returned from InsertOneAsync");
        return entity;
    }

    public async Task<Model.AuthState> GetAsync(string state)
    {
        // TODO: Add Try Catch in here like collections in creator
        _logger.LogDebug("Entered GetAsync function");
        _logger.LogInformation($"Parameter: state: {state}");

        var resp = await _container.AsQueryable().FirstOrDefaultAsync(x => x.Id == state);
        
        _logger.LogDebug("Returned from ReadItemAsync");
        return resp;
    }

    public async Task<Model.AuthState> DeleteAsync(string state)
    {
        _logger.LogDebug("Entered DeleteAsync function");
        _logger.LogInformation($"Parameter: state: {state}");

        return await _container.FindOneAndDeleteAsync(x => x.Id == state);
    }
}