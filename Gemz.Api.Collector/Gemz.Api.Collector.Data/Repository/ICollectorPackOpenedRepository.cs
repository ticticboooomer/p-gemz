using Gemz.Api.Collector.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Repository
{
    public interface ICollectorPackOpenedRepository
    {
        Task<CollectorPackOpened> CreateAsync(CollectorPackOpened entity);

        Task<List<CollectorPackOpened>> GetAllAsync();

        Task<bool> ReplaceAsync(CollectorPackOpened entity);

        Task<CollectorPackOpened> FetchOpenedPackForOrderLineAsync(string orderId, string orderLineId);

    }
}
