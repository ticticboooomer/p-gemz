using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Service.Collector.Model;

namespace Gemz.Api.Collector.Service.Collector;

public interface ICollectionService
{
    Task<GenericResponse<CollectionsListModel>> GetAllCollectionsForCreator(CreatorIdModel creatorIdModel);

    Task<GenericResponse<CollectionsPagedModel>> GetPagedCollectionsForCreator(CollectionPagingModel collectionPagingModel);

    Task<GenericResponse<CollectionModel>> GetSingleCollection(CollectionIdModel collectionIdModel);
}