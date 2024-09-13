using Gemz.Api.Creator.Service.Creator.Model;

namespace Gemz.Api.Creator.Service.Creator;

public interface ICollectionService
{
    Task<GenericResponse<CreatorCollections>> FetchPageOfCreatorCollections(string creatorId, CollectionPagingModel collectionPaging);

    Task<GenericResponse<CollectionModel>> CreateCollection(CollectionModel collectionModel, string creatorId);
    Task<GenericResponse<CollectionModel>> FetchCollectionById(CollectionIdModel collectionIdModel, string creatorId);

    Task<GenericResponse<CollectionModel>> UpdatePublishedStatusForCollection(CollectionIdModel collectionIdModel, string creatorId, int publishedStatus);
    Task<GenericResponse<CollectionModel>> UpdateCollection(CollectionUpdateModel collection, string creatorId);
    Task<GenericResponse<ArchiveCollectionOutputModel>> ArchiveCollection(CollectionIdModel collectionIdModel, string creatorId);
}