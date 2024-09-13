using Gemz.Api.StripeWebhook.Service.StripeWebhook;
using Gemz.Api.StripeWebhook.Service.StripeWebhook.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gemz.Api.StripeWebhook.Service
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<IStripeWebhookService, StripeWebhookService>();
            services.Configure<StripeConfig>(config.GetSection("Stripe"));
            return services;
        }
    }
}
