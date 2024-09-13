using Gemz.Api.Collector.Data.Model;

namespace Gemz.Api.Collector.Data.Repository;

public interface ICollectionRepository
{
    Task<List<Collection>> GetAllCollectionsByCreatorId(string creatorId);
    Task<CollectionsPage> GetCollectionsPageByCreatorId(string creatorId,  int currentPage, int pageSize);

    Task<Collection> GetSingleCollection(string collectionId);

    Task<Collection> GetSingleCollectionAnyStatus(string collectionId);
}