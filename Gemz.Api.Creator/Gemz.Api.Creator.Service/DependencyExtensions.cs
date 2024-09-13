using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gemz.Api.Creator.Service;

public static class DependencyExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<ICollectionService, CollectionService>();
        services.AddTransient<IGemService, GemService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<IStoreService, StoreService>();
        services.AddTransient<IDashboardService, DashboardService>();
        services.AddTransient<IRevealService, RevealService>();
        services.Configure<StripeConfig>(config.GetSection("Stripe"));
        services.Configure<GemzDefaultsConfig>(config.GetSection("GemzDefaults"));
        services.AddTransient<IStripeService, StripeService>();
        services.AddTransient<IOverlayService, OverlayService>();
        services.AddTransient<IInterestService, InterestService>();
        return services;
    }
    
}