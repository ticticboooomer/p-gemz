using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gemz.Api.Collector.Service;

public static class DependencyExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<ICollectionService, CollectionService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<IBasketService, BasketService>();
        services.AddTransient<IStoreService, StoreService>();
        services.AddTransient<IGemService, GemService>();
        services.AddTransient<ICheckoutService, CheckoutService>();
        services.AddTransient<IOrderService, OrderService>();
        services.AddTransient<ICollectorPackService, CollectorPackService>();
        services.AddTransient<IPurchasesService, PurchasesService>();
        services.AddTransient<IDashboardService, DashboardService>();
        services.Configure<StripeConfig>(config.GetSection("Stripe"));
        services.Configure<GemzDefaultsConfig>(config.GetSection("GemzDefaults"));
        return services;
    }
    
}