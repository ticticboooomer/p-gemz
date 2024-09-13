using Gemz.Api.Creator.Data.Factory;
using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Data.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;

namespace Gemz.Api.Creator.Data;

public static class DependencyExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection s, IConfiguration config)
    {
        var pack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("My Solution Conventions", pack, t => true);

        s.Configure<DbConfig>(config.GetSection("DatabaseConfig"));
        s.Configure<BlobConfig>(config.GetSection("BlobStorage"));
        s.AddSingleton<MongoFactory>();
        s.AddSingleton<BlobFactory>();
        s.AddTransient<ICollectionRepository, CollectionRepository>();
        s.AddTransient<IGemRepository, GemRepository>();
        s.AddTransient<IImageRepository, ImageRepository>();
        s.AddTransient<IStoreRepository, StoreRepository>();
        s.AddTransient<IStoreTagRepository, StoreTagRepository>();
        s.AddTransient<ICollectorGemRepository, CollectorGemRepository>();
        s.AddTransient<ICreatorToOpenRepository, CreatorToOpenRepository>();
        s.AddTransient<IAccountRepository, AccountRepository>();
        s.AddTransient<IOverlayKeyRepository, OverlayKeyRepository>();
        s.AddTransient<IRegisteredInterestRepository, RegisteredInterestRepository>();
        return s;
    }
}