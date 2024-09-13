using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Gemz.ServiceBus.Factory;

namespace Gemz.ServiceBus;
public static class DependencyExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ServiceBusConfig>(config.GetSection("ServiceBus"));
        return services;
    }
}
