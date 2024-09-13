using System.Text.Json;
using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Creator.Service.Creator;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _collectionRepo;
    private readonly IGemRepository _gemRepo;
    private readonly ICollectorGemRepository _collectorGemRepo;
    private readonly ILogger<CollectionService> _logger;


    public CollectionService(ICollectionRepository collectionRepo, 
                            IGemRepository gemRepo, 
                            ICollectorGemRepository collectorGemRepo,
                            ILogger<CollectionService> logger)
    {
        _collectionRepo = collectionRepo;
        _gemRepo = gemRepo;
        _collectorGemRepo = collectorGemRepo;
        _logger = logger;
    }

    #region Service_Methods

    public async Task<GenericResponse<CreatorCollections>> FetchPageOfCreatorCollections(string? creatorId,  CollectionPagingModel collectionPaging)
    {
        _logger.LogDebug("Entered FetchPageOfCreatorCollections function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<CreatorCollections>()
            {
                Error = "CR000001"
            };
        }     
        
        if (collectionPaging == null)
        {
            _logger.LogError("Paging parameters passed in as null object, leaving function.");
            return new GenericResponse<CreatorCollections>()
            {
                Error = "CR000002"
            };
        }

        if (collectionPaging.PageSize <= 0)
        {
            _logger.LogError("pageSize passed in as <= zero, leaving function.");
            return new GenericResponse<CreatorCollections>()
            {
                Error = "CR000003"
            };
        }

        if (collectionPaging.CurrentPage < 0)
        {
            _logger.LogError("currentPage passed in as < zero, leaving function.");
            return new GenericResponse<CreatorCollections>()
            {
                Error = "CR001000"
            };
        }

        _logger.LogDebug("Calling Creator Repo GetCollectionsPageByCreatorId");
        var collectionsPage = await _collectionRepo.GetCollectionsPageByCreatorId(creatorId, collectionPaging.CurrentPage, collectionPaging.PageSize);

        if (collectionsPage == null)
        {
            _logger.LogError("null returned from repo function GetCollectionsPageByCreatorId. Leaving function, returning null.");
            return new GenericResponse<CreatorCollections>()
            {
                Error = "CR000004"
            };
        }

        _logger.LogInformation($"Repo returned {collectionsPage.Collections.Count} collection records.");
        var creatorCollections = new CreatorCollections()
        {
            Collections = new List<CollectionModel>(),
            ThisPage = collectionsPage.ThisPage,
            TotalPages = collectionsPage.TotalPages
        };

        _logger.LogDebug("Building CreatorCollections return type from repo data.");
        foreach (var collection in collectionsPage.Collections)
        {
            var collectionModel = MapCollectionToCollectionModel(collection);
            creatorCollections.Collections.Add(collectionModel);
        }
        
        _logger.LogDebug("Return data built, returning data.");
        return new GenericResponse<CreatorCollections>()
        {
            Data = creatorCollections
        };
    }

    public async Task<GenericResponse<CollectionModel>> CreateCollection(CollectionModel collectionModel, string creatorId)
    {
        _logger.LogDebug("Entered CreateCollection in Service.");

        if (collectionModel == null)
        {
            _logger.LogError("No collection data passed in to Create end point. Exiting.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000005"
            };
        }

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No Creator Id passed in to Create end point. Exiting.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000007"
            };
        }

        if (string.IsNullOrEmpty(collectionModel.Name) || collectionModel.Name.Length > 100)
        {
            _logger.LogError("Collection Name is either empty or more than 100 chars. Exiting.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000040"
            };
        }

        if (collectionModel.ClusterPrice < 1)
        {
            _logger.LogError("ClusterPrice is < 1 so failed validation. Exiting.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000041"
            };
        }
        
        var newCollection = new Collection()
        {
            Id = Guid.NewGuid().ToString(), 
            CreatorId = creatorId,
            Name = collectionModel.Name,
            PublishedStatus = 0,
            ClusterPrice = collectionModel.ClusterPrice,
            Deleted = false,
            CreatedOn = DateTime.UtcNow
        };

        var createdCollection = await _collectionRepo.CreateAsync(newCollection);

        if (createdCollection == null)
        {
            _logger.LogError("Create of Collection failed. Leaving function with null.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000006"
            };
        }
        
        _logger.LogDebug("Created new Collection.");
        _logger.LogInformation($"New Collection Id: {createdCollection.Id}");
        
        var collectionData = MapCollectionToCollectionModel(createdCollection);

        return new GenericResponse<CollectionModel>()
        {
            Data = collectionData
        };
    }


    public async Task<GenericResponse<CollectionModel>> FetchCollectionById(CollectionIdModel collectionIdModel,
        string creatorId)
    {
        _logger.LogDebug("Entered FetchCollectionById function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000011"
            };
        }

        if (string.IsNullOrEmpty(collectionIdModel.CollectionId))
        {
            _logger.LogError("No Collection Id Detected. Leaving function.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000008"
            };
        }
        
        _logger.LogInformation($"Fetching data for collection with Id {collectionIdModel.CollectionId.ToString()} and Creator Id {creatorId}");

        var collection = await _collectionRepo.GetAsync(collectionIdModel.CollectionId);

        if (collection == null)
        {
            _logger.LogError("Repo returned null collection");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000010"
            };
        }

        if (collection.CreatorId != creatorId)
        {
            _logger.LogError("Repo returned collection is for a different Creator Id");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000009"
            };
        }

        var collectionData = MapCollectionToCollectionModel(collection);
        return new GenericResponse<CollectionModel>()
        {
            Data = collectionData
        };
    }

    public async Task<GenericResponse<CollectionModel>> UpdatePublishedStatusForCollection(CollectionIdModel collectionIdModel, string creatorId, int publishedStatus)
    {
        _logger.LogDebug("Entered UpdatePublishedStatusForCollection function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000012"
            };
        }

        if (string.IsNullOrEmpty(collectionIdModel.CollectionId))
        {
            _logger.LogError("No Collection Id Detected. Leaving function.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000013"
            };
        }
        
        _logger.LogInformation($"Fetching data for collection with Id {collectionIdModel.CollectionId} and Creator Id {creatorId}");

        var collection = await _collectionRepo.GetAsync(collectionIdModel.CollectionId);

        if (collection == null)
        {
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000014"
            };
        }

        if (collection.CreatorId != creatorId)
        {
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000015"
            };
        }

        var patchedCollection = collection;
        var publishDenied = false;

        if (publishedStatus == 1)
        {
            if (!await _gemRepo.AnyPublishedGemsInCollection(collection.Id))
            {
                _logger.LogWarning("Unable to publish this collection as contains zero published Gems");
                publishDenied = true;
            }
        }


        if (!publishDenied)
        {
            _logger.LogInformation($"Updating PulishedStatus to {publishedStatus} for collection with Id {collectionIdModel.CollectionId} and Creator Id {creatorId}");

            var success = await _collectionRepo.PatchItemPublishedStatusAsync(collection.Id, publishedStatus);

            if (!success)
            {
                _logger.LogError("Patch on PublishedStatus of Collection failed");
                return new GenericResponse<CollectionModel>()
                {
                    Error = "CR000016"
                };
            }
            patchedCollection.PublishedStatus = publishedStatus;
        }

        var collectionModel = MapCollectionToCollectionModel(patchedCollection, publishDenied);
        return new GenericResponse<CollectionModel>()
        {
            Data = collectionModel
        };
    }

    public async Task<GenericResponse<CollectionModel>> UpdateCollection(CollectionUpdateModel collectionUpdateModel, string creatorId)
    {
        _logger.LogDebug("Entered UpdateCollection function");
        
        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000029"
            };
        }
        
        if (string.IsNullOrEmpty(collectionUpdateModel.Id))
        {
            _logger.LogError("No Collection Id Detected. Leaving function.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000030"
            };
        }
        
        if (string.IsNullOrEmpty(collectionUpdateModel.Name) || collectionUpdateModel.Name.Length > 100)
        {
            _logger.LogError("Collection Name is either empty or more than 100 chars. Exiting.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000046"
            };
        }

        if (collectionUpdateModel.ClusterPrice < 1)
        {
            _logger.LogError("ClusterPrice is < 1 so failed validation. Exiting.");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000047"
            };
        }

        _logger.LogInformation($"Fetching data for collection with Id {collectionUpdateModel.Id} and Creator Id {creatorId}");
        
        var collection = await _collectionRepo.GetAsync(collectionUpdateModel.Id);
        
        if (collection == null)
        {
            _logger.LogError("Unable to retrieve the collection to be updated");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000031"
            };
        }
        
        if (collection.CreatorId != creatorId)
        {
            _logger.LogError("Creator Id for existing collection does note match Creator Id passed in");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000032"
            };
        }
        
        _logger.LogInformation($"Updating collection with Id {collectionUpdateModel.Id} and Creator Id {creatorId}");

        collection.Name = collectionUpdateModel.Name;
        collection.ClusterPrice = collectionUpdateModel.ClusterPrice;
        
        var updatedCollection = await _collectionRepo.UpdateCollectionAsync(collection);
        
        
        if (updatedCollection is null)
        {
            _logger.LogError("Problem during update operation of collection in database");
            return new GenericResponse<CollectionModel>()
            {
                Error = "CR000035"
            };
        }

        var collectionModel = MapCollectionToCollectionModel(updatedCollection);
        return new GenericResponse<CollectionModel>()
        {
            Data = collectionModel
        };
    }

    public async Task<GenericResponse<ArchiveCollectionOutputModel>> ArchiveCollection(CollectionIdModel collectionIdModel, string creatorId)
    {
        _logger.LogDebug("Entered ArchiveCollection function");
        
        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<ArchiveCollectionOutputModel>()
            {
                Error = "CR000600"
            };
        }
        
        if (string.IsNullOrEmpty(collectionIdModel.CollectionId))
        {
            _logger.LogError("No Collection Id Detected. Leaving function.");
            return new GenericResponse<ArchiveCollectionOutputModel>()
            {
                Error = "CR000601"
            };
        }

        _logger.LogInformation($"Fetching data for collection with Id {collectionIdModel.CollectionId} and Creator Id {creatorId}");
        
        var collection = await _collectionRepo.GetAsync(collectionIdModel.CollectionId);
        
        if (collection == null)
        {
            _logger.LogError("Unable to retrieve the collection to be updated");
            return new GenericResponse<ArchiveCollectionOutputModel>()
            {
                Error = "CR000602"
            };
        }
        
        if (collection.CreatorId != creatorId)
        {
            _logger.LogError("Creator Id for existing collection does note match Creator Id passed in");
            return new GenericResponse<ArchiveCollectionOutputModel>()
            {
                Error = "CR000603"
            };
        }

        _logger.LogDebug("Check to see if any collectors have purchased from this collection");

        var collectorGem = await _collectorGemRepo.GetFirstForCollection(creatorId, collectionIdModel.CollectionId);

        if (collectorGem != null)
        {
            _logger.LogError("Cannot Archive this collection as Collector has purchased from it");
            return new GenericResponse<ArchiveCollectionOutputModel>
            {
                Data = new ArchiveCollectionOutputModel
                {
                    ArchiveCompleted = false,
                    ArchiveDenied = true
                }
            };
        }


        _logger.LogInformation($"Archiving collection with Id {collectionIdModel.CollectionId} and Creator Id {creatorId}");

        collection.Deleted = true;
        var success = await _collectionRepo.PatchCollectionDeletedAsync(collectionIdModel.CollectionId);

        if (!success)
        {
            _logger.LogError("Error occured whilst marking the collection as deleted.");
            return new GenericResponse<ArchiveCollectionOutputModel>()
            {
                Error = "CR000604"
            };
        }

        return new GenericResponse<ArchiveCollectionOutputModel>()
        {
            Data = new ArchiveCollectionOutputModel
            {
                ArchiveCompleted = true,
                ArchiveDenied = false
            }
        };
    }
    
    #endregion

    #region Private Classes

    private static CollectionModel MapCollectionToCollectionModel(Collection collection, bool publishedDenied = false)
    {
        return new CollectionModel()
        {
            Id = collection.Id,
            CreatorId = collection.CreatorId,
            Name = collection.Name,
            ClusterPrice = collection.ClusterPrice,
            PublishedStatus = collection.PublishedStatus,
            CreatedOn = collection.CreatedOn,
            Deleted = collection.Deleted,
            PublishDenied = publishedDenied
        };
    }

    #endregion

}
