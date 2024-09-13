using Gemz.Api.Collector.Data.Factory;
using Gemz.Api.Collector.Data.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using Stripe;

namespace Gemz.Api.Collector.Data;

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
        s.AddTransient<IImageRepository, ImageRepository>();
        s.AddTransient<IBasketRepository, BasketRepository>();
        s.AddTransient<IStoreTagRepository, StoreTagRepository>();
        s.AddTransient<IStoreRepository, StoreRepository>();
        s.AddTransient<IGemRepository, GemRepository>();
        s.AddTransient<IOrderRepository, OrderRepository>();
        s.AddTransient<IStripePaymentIntentRepository, StripePaymentIntentRepository>();
        s.AddTransient<ICollectorPackRepository, CollectorPackRepository>();
        s.AddTransient<ICollectorPackOpenedRepository, CollectorPackOpenedRepository>();
        s.AddTransient<ICollectorGemRepository, CollectorGemRepository>();
        s.AddTransient<ICreatorToOpenRepository, CreatorToOpenRepository>();
        s.AddTransient<IOpenPackSessionRepository, OpenPackSessionRepository>();
        s.AddTransient<IAccountRepository, AccountRepository>();
        return s;
    }
}