using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository
{
    public interface ICollectorGemRepository
    {
        Task<CollectorGem> CreateAsync(CollectorGem entity);

        Task<List<CollectorGem>> GetForSingleCollectorAsync(string collectorId);

        Task<List<CollectorGem>> GetForSingleCreatorAndCollector(string collectorId, string creatorId);
        Task<List<CollectorGem>> GetForSingleCollectorAndCollection(string collectorId, string collectionId);
    }
}
