using Gemz.ServiceBus.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.ServiceBus
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddServiceBusServices(this IServiceCollection s, IConfiguration config)
        {
            s.Configure<ServiceBusConfig>(config.GetSection("ServiceBus"));
            return s;
        }
    }
}
