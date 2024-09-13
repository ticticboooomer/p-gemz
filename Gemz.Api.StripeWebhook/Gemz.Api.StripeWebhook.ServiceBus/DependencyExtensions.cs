using Gemz.ServiceBus.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gemz.ServiceBus
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddServiceBusServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ServiceBusConfig>(config.GetSection("ServiceBus"));
            return services;
        }
    }
}
