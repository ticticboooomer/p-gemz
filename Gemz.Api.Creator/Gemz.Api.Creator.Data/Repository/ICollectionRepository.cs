using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository;

public interface ICollectionRepository
{
    Task<CollectionsPage> GetCollectionsPageByCreatorId(string creatorId,  int currentPage, int pageSize);

    Task<Collection> CreateAsync(Collection entity);

    Task<Collection> GetAsync(string collectionId);
    Task<Collection> GetAsyncAnyStatus(string collectionId);

    Task<bool> PatchItemPublishedStatusAsync(string collectionId, int publishedStatus);

    Task<Collection> UpdateCollectionAsync(Collection entity);

    Task<bool> PatchCollectionDeletedAsync(string collectionId);

    Task<List<Collection>> GetAllCollectionsByCreatorId(string creatorId);
}