using Gemz.Api.Collector.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemz.Api.Collector.Data.Repository
{
    public interface ICollectorPackRepository
    {
        Task<CollectorPack> CreateAsync(CollectorPack entity);
        Task<List<CollectorPack>> FetchUnopenedPacksForCollector(string collectorId);
        Task<List<CollectorPack>> FetchUnopenedPacksInCollection(string collectorId, string collectionId);
        Task<bool> DeleteAsync(string collectorPackId);

        Task<CollectorPack> FetchPackForOrderLineAsync(string orderId, string orderLineId);
    }
}
