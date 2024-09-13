using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Collector.Service.Collector;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _collectionRepo;
    private readonly IStoreTagRepository _storeTagRepo;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(ICollectionRepository collectionRepo, IStoreTagRepository storeTagRepo, ILogger<CollectionService> logger)
    {
        _collectionRepo = collectionRepo;
        _storeTagRepo = storeTagRepo;
        _logger = logger;
    }
    
    public async Task<GenericResponse<CollectionsListModel>> GetAllCollectionsForCreator(CreatorIdModel creatorIdModel)
    {
        _logger.LogDebug("Entered FetchPageOfCreatorCollections function");

        if (string.IsNullOrEmpty(creatorIdModel.CreatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<CollectionsListModel>()
            {
                Error = "CL000101"
            };
        }     
        
        var collectionsList = await _collectionRepo.GetAllCollectionsByCreatorId(creatorIdModel.CreatorId);

        if (collectionsList is null)
        {
            _logger.LogError("null returned from repo function GetAllCollectionsByCreatorId. Leaving function, returning null.");
            return new GenericResponse<CollectionsListModel>()
            {
                Error = "CL000102"
            };
        }

        _logger.LogInformation($"Repo returned {collectionsList.Count} collection records.");
        var collectionsListModel = new CollectionsListModel()
        {
            Collections = new List<CollectionModel>()
        };

        _logger.LogDebug("Building CollectionsListModel return type from repo data.");
        foreach (var collection in collectionsList)
        {
            var collectionModel = MapCollectionToCollectionModel(collection);
            collectionsListModel.Collections.Add(collectionModel);
        }
        
        _logger.LogDebug("Return data built, returning data.");
        return new GenericResponse<CollectionsListModel>()
        {
            Data = collectionsListModel
        };
    }

    public async Task<GenericResponse<CollectionsPagedModel>> GetPagedCollectionsForCreator(CollectionPagingModel collectionPagingModel)
    {
        _logger.LogDebug("Entered FetchPageOfCreatorCollections function");

        if (collectionPagingModel is null)
        {
            _logger.LogError("Paging parameters passed in as null object, leaving function.");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000002"
            };
        }

        if (string.IsNullOrEmpty(collectionPagingModel.StoreTag))
        {
            _logger.LogError("No StoreTag Detected. Leaving function.");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000001"
            };
        }

        if (collectionPagingModel.StoreTag.Length > 30)
        {
            _logger.LogError("StoreTag > 30 chars. Leaving function.");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000007"
            };
        }
        
        if (collectionPagingModel.StoreTag.Length < 3)
        {
            _logger.LogError("StoreTag < 3 chars. Leaving function.");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000008"
            };
        }
        
        if (collectionPagingModel.PageSize <= 0)
        {
            _logger.LogError("pageSize passed in as <= zero. Leaving function.");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000003"
            };
        }

        if (collectionPagingModel.CurrentPage < 0 )
        {
            _logger.LogError("CurrentPage passed in as < zero. Leaving function.");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000009"
            };
        }

        var storeTag = await _storeTagRepo.GetAsync(collectionPagingModel.StoreTag.ToLower());
        if (storeTag is null)
        {
            _logger.LogError("Store Tag does not exist");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000005"
            };
        }

        if (string.IsNullOrEmpty(storeTag.CreatorId))
        {
            _logger.LogError("Store Tag does not have a creatorId");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000006"
            };
        }
        
        _logger.LogDebug("Calling Creator Repo GetCollectionsPageByCreatorId");
        var collectionsPage = await _collectionRepo.GetCollectionsPageByCreatorId(storeTag.CreatorId, collectionPagingModel.CurrentPage, collectionPagingModel.PageSize);

        if (collectionsPage is null)
        {
            _logger.LogError("null returned from repo function GetCollectionsPageByCreatorId. Leaving function, returning null.");
            return new GenericResponse<CollectionsPagedModel>()
            {
                Error = "CL000004"
            };
        }

        _logger.LogInformation($"Repo returned {collectionsPage.Collections.Count} collection records.");
        var collectionsPagedModel = new CollectionsPagedModel()
        {
            Collections = new List<CollectionModel>(),
            ThisPage = collectionsPage.ThisPage,
            TotalPages = collectionsPage.TotalPages
        };

        _logger.LogDebug("Building CollectionsPagedModel return type from repo data.");
        foreach (var collection in collectionsPage.Collections)
        {
            collectionsPagedModel.Collections.Add(MapCollectionToCollectionModel(collection));
        }
        
        _logger.LogDebug("Return data built, returning data.");
        return new GenericResponse<CollectionsPagedModel>()
        {
            Data = collectionsPagedModel
        };
    }

    public async Task<GenericResponse<CollectionModel>> GetSingleCollection(CollectionIdModel collectionIdModel)
    {
        if (collectionIdModel is null)
        {
            _logger.LogError("CollectionIdModel passed in was null");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CL001900"
            };
        }

        if (string.IsNullOrEmpty(collectionIdModel.CollectionId))
        {
            _logger.LogError("CollectionId passed in was null or empty");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CL001901"
            };
        }

        var collection = await _collectionRepo.GetSingleCollection(collectionIdModel.CollectionId);

        if (collection is null)
        {
            _logger.LogError("Collection does not exist in Repo");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CL001902"
            };
        }

        return new GenericResponse<CollectionModel>()
        {
            Data = MapCollectionToCollectionModel(collection)
        };
    }

    
    private static CollectionModel MapCollectionToCollectionModel(Collection collection)
    {
        return new CollectionModel()
        {
            CollectionId = collection.Id,
            Name = collection.Name,
            ClusterPrice = collection.ClusterPrice
        };
    }
}