using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Gemz.Api.Creator.Data.Factory;

public class MongoFactory
{
    private readonly IOptions<DbConfig> _config;
    private readonly MongoClient _mongoClient;

    public MongoFactory(IOptions<DbConfig> config)
    {
        _config = config;
        _mongoClient = new MongoClient(_config.Value.Connection);
    }

    public IMongoDatabase GetDatabase()
    {
        return _mongoClient.GetDatabase(_config.Value.Database);
    }
}