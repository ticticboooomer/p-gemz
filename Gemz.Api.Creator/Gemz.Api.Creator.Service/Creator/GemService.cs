using System.Text.Json;
using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Creator.Service.Creator;

public class GemService : IGemService
{
    private readonly IGemRepository _gemRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IImageRepository _imageRepo;
    private readonly ICollectorGemRepository _collectorGemRepo;
    private readonly ILogger<GemService> _logger;

    public GemService(IGemRepository gemRepository, 
                        ICollectionRepository collectionRepo, 
                        IImageRepository imageRepo, 
                        ICollectorGemRepository collectorGemRepo,
                        ILogger<GemService> logger)
    {
        _gemRepo = gemRepository;
        _collectionRepo = collectionRepo;
        _imageRepo = imageRepo;
        _collectorGemRepo = collectorGemRepo;
        _logger = logger;
    }
    
    public async Task<GenericResponse<FetchGemOutputModel>> FetchGemById(GemIdModel gemIdModel, string creatorId)
    {
        _logger.LogDebug("Entered FetchGemById function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<FetchGemOutputModel>()
            {
                Error = "CR000017"
            };
        }

        if (string.IsNullOrEmpty(gemIdModel.GemId))
        {
            _logger.LogError("No Gem Id Detected. Leaving function.");
            return new GenericResponse<FetchGemOutputModel>()
            {
                Error = "CR000018"
            };
        }
        
        _logger.LogInformation($"Fetching data for Gem with Id {gemIdModel.GemId.ToString()} and Creator Id {creatorId}");

        var gem = await _gemRepo.GetAsync(gemIdModel.GemId);

        if (gem == null)
        {
            return new GenericResponse<FetchGemOutputModel>()
            {
                Error = "CR000019"
            };
        }

        if (gem.CreatorId != creatorId)
        {
            return new GenericResponse<FetchGemOutputModel>()
            {
                Error = "CR000020"
            };
        }

        var gemData = MapGemToFetchGemOutputModel(gem);
        return new GenericResponse<FetchGemOutputModel>()
        {
            Data = gemData
        };
    }

    public async Task<GenericResponse<CreateGemOutputModel>> CreateGem(GemModel gemModel, string creatorId)
    {
        _logger.LogDebug("Entered CreateGem in Service.");

        if (gemModel == null)
        {
            _logger.LogError("No gem data passed in to CreateGem Service. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000021"
            };
        }

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No Creator Id passed in to CreateGem Service. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000022"
            };
        }

        if (string.IsNullOrEmpty(gemModel.CollectionId))
        {
            _logger.LogError("No Collection Id passed in to CreateGem Service. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000024"
            };
        }

        if (string.IsNullOrEmpty(gemModel.Name) || gemModel.Name.Length > 100)
        {
            _logger.LogError("Name field was either empty of > 100 chars. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000036"
            };
        }

        if (gemModel.Rarity is < 0 or > 4)
        {
            _logger.LogError("Rarity field was < 0 or > 4. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000039"
            };
        }

        if (string.IsNullOrEmpty(gemModel.ImageId))
        {
            _logger.LogError("Image Id field was null or empty. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000037"
            };
        }

        if (gemModel.SizePercentage is < 1 or > 100)
        {
            _logger.LogError("SizePercentage field was < 1 or > 100. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000043"
            };
        }

        if (gemModel.PositionXPercentage is < 1 or > 100)
        {
            _logger.LogError("Position X Percentage field is < 1 or > 100. Existing.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000044"
            };
        }

        if (gemModel.PositionYPercentage is < 1 or > 100)
        {
            _logger.LogError("Position Y Percentage field is < 1 or > 100. Existing.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000045"
            };
        }

        var isCollectionForCreator = await CheckCollectionIsForThisCreator(gemModel.CollectionId, creatorId);

        if (!isCollectionForCreator)
        {
            _logger.LogError("Collection passed in does not belong to Creator Id passed in. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000034"
            };
        }
        
        var imageExistsAndOwnedByCreator = await ValidateImageExistsAndOwnedByCreator(gemModel.ImageId, creatorId);
        if (!imageExistsAndOwnedByCreator)
        {
            _logger.LogError("Image in Image Id field doesn't exist or doesn't belong to this creator. Exiting.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000038"
            };
        }
        
        var newGem = new Gem()
        {
            Id = Guid.NewGuid().ToString(), 
            CreatorId = creatorId,
            CollectionId = gemModel.CollectionId,
            Name = gemModel.Name,
            Rarity = gemModel.Rarity,
            ImageId = gemModel.ImageId,
            SizePercentage = gemModel.SizePercentage,
            PositionXPercentage = gemModel.PositionXPercentage,
            PositionYPercentage = gemModel.PositionYPercentage,
            PublishedStatus = gemModel.PublishedStatus,
            CreatedOn = DateTime.UtcNow
        };

        var createdGem = await _gemRepo.CreateAsync(newGem);

        if (createdGem == null)
        {
            _logger.LogError("Create of Gem failed. Leaving function with null.");
            return new GenericResponse<CreateGemOutputModel>()
            {
                Error = "CR000023"
            };
        }
        
        _logger.LogDebug("Created new Gem.");
        _logger.LogInformation($"New Gem Id: {createdGem.Id}");
        
        var gemData = MapGemToCreateGemOutputModel(createdGem);

        return new GenericResponse<CreateGemOutputModel>()
        {
            Data = gemData
        };
    }

    public async Task<GenericResponse<UpdateStatusGemOutputModel>> UpdatePublishedStatusForGem(GemIdModel gemIdModel, string creatorId, int publishedStatus)
    {
        _logger.LogDebug("Entered UpdatePublishedStatusForGem function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<UpdateStatusGemOutputModel>()
            {
                Error = "CR000112"
            };
        }

        if (string.IsNullOrEmpty(gemIdModel.GemId))
        {
            _logger.LogError("No Gem Id Detected. Leaving function.");
            return new GenericResponse<UpdateStatusGemOutputModel>()
            {
                Error = "CR000113"
            };
        }
        
        _logger.LogInformation($"Fetching data for gem with Id {gemIdModel.GemId} and Creator Id {creatorId}");

        var gem = await _gemRepo.GetAsync(gemIdModel.GemId);

        if (gem == null)
        {
            return new GenericResponse<UpdateStatusGemOutputModel>()
            {
                Error = "CR000114"
            };
        }

        if (gem.CreatorId != creatorId)
        {
            return new GenericResponse<UpdateStatusGemOutputModel>()
            {
                Error = "CR000115"
            };
        }

        _logger.LogInformation($"Publishing gem with Id {gemIdModel.GemId} and Creator Id {creatorId}");

        var patchedGemSuccess = await _gemRepo.PatchItemPublishedStatusAsync(gem.Id, publishedStatus);
        

        if (!patchedGemSuccess)
        {
            return new GenericResponse<UpdateStatusGemOutputModel>()
            {
                Error = "CR000116"
            };
        }

        gem.PublishedStatus = publishedStatus;

        var autoUnpublishedCollection = false;

        if (gem.PublishedStatus == 0) { 
            if (!await _gemRepo.AnyPublishedGemsInCollection(gem.CollectionId))
            {
                var collectionUnpublished = await _collectionRepo.PatchItemPublishedStatusAsync(gem.CollectionId, 0);
                if (collectionUnpublished)
                {
                    autoUnpublishedCollection = true;
                }
            }
        }

        var gemModel = MapGemToUpdateStatusGemOutputModel(gem, autoUnpublishedCollection); 
        return new GenericResponse<UpdateStatusGemOutputModel>()
        {
            Data = gemModel
        };
    }

    public async Task<GenericResponse<UpdateGemOutputModel>> UpdateGem(GemUpdateModel gemUpdateModel, string creatorId)
    {
        _logger.LogDebug("Entered UpdateGem function");
        
        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000129"
            };
        }
        
        if (string.IsNullOrEmpty(gemUpdateModel.Id))
        {
            _logger.LogError("No Gem Id Detected. Leaving function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000130"
            };
        }

        if (string.IsNullOrEmpty(gemUpdateModel.Name) || gemUpdateModel.Name.Length > 100)
        {
            _logger.LogError("Name field failed validation. Leaving function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000136"
            };
        }

        if (gemUpdateModel.Rarity is < 0 or > 4)
        {
            _logger.LogInformation($"Rarity: {gemUpdateModel.Rarity}");
            _logger.LogError("Rarity field < 0 or > 4 so failed validation. Leaving function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000137"
            };
        }

        if (string.IsNullOrEmpty(gemUpdateModel.ImageId))
        {
            _logger.LogError("Image Id field is empty. Leaving function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000138"
            };
        }
        
        var imageExistsAndOwnedByCreator = await ValidateImageExistsAndOwnedByCreator(gemUpdateModel.ImageId, creatorId);
        if (!imageExistsAndOwnedByCreator)
        {
            _logger.LogError("Image in Image Id field doesn't exist or doesn't belong to this creator. Exiting.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000139"
            };
        }

        if (gemUpdateModel.SizePercentage is < 1 or > 100)
        {
            _logger.LogError("SizePercentage field was < 1 or > 100. Leaving Function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000140"
            };
        }

        if (gemUpdateModel.PositionXPercentage is < 1 or > 100)
        {
            _logger.LogError("Position X Percentage field is < 1 or > 100. Leaving Function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000141"
            };
        }

        if (gemUpdateModel.PositionYPercentage is < 1 or > 100)
        {
            _logger.LogError("Position Y Percentage field is < 1 or > 100. Leaving Function.");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000142"
            };
        }

        _logger.LogInformation($"Fetching data for gem with Id {gemUpdateModel.Id} and Creator Id {creatorId}");
        
        var gem = await _gemRepo.GetAsync(gemUpdateModel.Id);
        
        if (gem == null)
        {
            _logger.LogError("Unable to retrieve the gem to be updated");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000131"
            };
        }
        
        if (gem.CreatorId != creatorId)
        {
            _logger.LogError("Creator Id for existing gem does note match Creator Id passed in");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000132"
            };
        }
        
        _logger.LogInformation($"Updating gem with Id {gemUpdateModel.Id} and Creator Id {creatorId}");

        gem.Name = gemUpdateModel.Name;
        gem.Rarity = gemUpdateModel.Rarity;
        gem.ImageId = gemUpdateModel.ImageId;
        gem.SizePercentage = gemUpdateModel.SizePercentage;
        gem.PositionXPercentage = gemUpdateModel.PositionXPercentage;
        gem.PositionYPercentage = gemUpdateModel.PositionYPercentage;
        
        var updatedGem = await _gemRepo.UpdateGemAsync(gem);
        
        
        if (updatedGem is null)
        {
            _logger.LogError("Problem during update operation of gem in database");
            return new GenericResponse<UpdateGemOutputModel>()
            {
                Error = "CR000135"
            };
        }

        var gemData = MapGemToUpdateGemOutputModel(updatedGem);
        return new GenericResponse<UpdateGemOutputModel>()
        {
            Data = gemData
        };
    }

    public async Task<GenericResponse<ArchiveGemOutputModel>> ArchiveGem(GemIdModel gemIdModel, string creatorId)
    {
        _logger.LogDebug("Entered ArchiveGem function");
        
        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<ArchiveGemOutputModel>()
            {
                Error = "CR000700"
            };
        }
        
        if (string.IsNullOrEmpty(gemIdModel.GemId))
        {
            _logger.LogError("No Gem Id Detected. Leaving function.");
            return new GenericResponse<ArchiveGemOutputModel>()
            {
                Error = "CR000701"
            };
        }

        _logger.LogInformation($"Fetching data for gem with Id {gemIdModel.GemId} and Creator Id {creatorId}");
        
        var gem = await _gemRepo.GetAsync(gemIdModel.GemId);
        
        if (gem == null)
        {
            _logger.LogError("Unable to retrieve the gem to be updated");
            return new GenericResponse<ArchiveGemOutputModel>()
            {
                Error = "CR000702"
            };
        }
        
        if (gem.CreatorId != creatorId)
        {
            _logger.LogError("Creator Id for existing gem does note match Creator Id passed in");
            return new GenericResponse<ArchiveGemOutputModel>()
            {
                Error = "CR000703"
            };
        }

        _logger.LogDebug("Check if Gem has been purchased at least once");

        var collectorGem = await _collectorGemRepo.GetFirstForGem(creatorId, gemIdModel.GemId);

        if (collectorGem != null)
        {
            _logger.LogWarning("Cannot Delete a Gem that has been purchased at least once");
            return new GenericResponse<ArchiveGemOutputModel>()
            {
                Data = new ArchiveGemOutputModel
                {
                    ArchiveCompleted = false,
                    ArchiveDenied = true
                }
            };
        }
        
        _logger.LogInformation($"Marking Updating gem with Id {gemIdModel.GemId} as deleted");

        gem.Deleted = true;
        
        var success = await _gemRepo.PatchGemDeletedAsync(gemIdModel.GemId);
        
        
        if (!success)
        {
            _logger.LogError("Problem during Patch Gem Deleted of gem in database");
            return new GenericResponse<ArchiveGemOutputModel>()
            {
                Error = "CR000704"
            };
        }

        return new GenericResponse<ArchiveGemOutputModel>()
        {
            Data = new ArchiveGemOutputModel
            {
                ArchiveCompleted = true,
                ArchiveDenied = false
            }
        };
    }

    public async Task<GenericResponse<GemCollectionModel>> FetchGemsInCollection(GemsPagingModel gemsPagingModel,
        string creatorId)
    {
        _logger.LogDebug("Entered FetchGemsInCollection function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<GemCollectionModel>()
            {
                Error = "CR000025"
            };
        }     
        
        
        if (gemsPagingModel.CurrentPage < 0)
        {
            _logger.LogError("currentPage parameter was invalid (< 0). Leaving function.");
            return new GenericResponse<GemCollectionModel>()
            {
                Error = "CR000026"
            };
        }

        if (gemsPagingModel.PageSize <= 0)
        {
            _logger.LogError("pageSize passed in as <= zero, leaving function.");
            return new GenericResponse<GemCollectionModel>()
            {
                Error = "CR000027"
            };
        }

        if (string.IsNullOrEmpty(gemsPagingModel.CollectionId))
        {
            _logger.LogError("No collectionId supplied. leaving function.");
            return new GenericResponse<GemCollectionModel>()
            {
                Error = "CR000042"
            };
        }

        var isCollectionForCreator = await CheckCollectionIsForThisCreator(gemsPagingModel.CollectionId, creatorId);

        if (!isCollectionForCreator)
        {
            _logger.LogError("Collection passed in does not belong to Creator Id passed in. Exiting.");
            return new GenericResponse<GemCollectionModel>()
            {
                Error = "CR000033"
            };
        }

        _logger.LogDebug("Calling Gems Repo GetGemsPageByCollectionId");
        var gemsPage = await _gemRepo.GetGemsPageByCollectionId(gemsPagingModel.CollectionId,creatorId, gemsPagingModel.CurrentPage, gemsPagingModel.PageSize);

        if (gemsPage == null)
        {
            _logger.LogError(
                "null returned from repo function GetGemsPageByCollectionId. Leaving function, returning null.");
            return new GenericResponse<GemCollectionModel>()
            {
                Error = "CR000028"
            };
        }
        
        _logger.LogInformation($"Repo returned {gemsPage.Gems.Count} Gems records.");
        var gemsCollection = new GemCollectionModel()
        {
            Gems = new List<GemModel>(),
            ThisPage = gemsPage.ThisPage,
            TotalPages = gemsPage.TotalPages
        };

        _logger.LogDebug("Building GemCollectionModel return type from repo data.");
        foreach (var gem in gemsPage.Gems)
        {
            var gemModel = MapGemToGemModel(gem);
            gemsCollection.Gems.Add(gemModel);
        }
        
        _logger.LogDebug("Return data built, returning data.");
        return new GenericResponse<GemCollectionModel>()
        {
            Data = gemsCollection
        };
    }
    
    private static GemModel MapGemToGemModel(Gem gem, bool autoUnpublishedCollection = false)
    {
        return new GemModel()
        {
            Id = gem.Id,
            CreatorId = gem.CreatorId,
            CollectionId = gem.CollectionId,
            Name = gem.Name,
            Rarity = gem.Rarity,
            ImageId = gem.ImageId,
            SizePercentage = gem.SizePercentage,
            PublishedStatus = gem.PublishedStatus,
            PositionXPercentage = gem.PositionXPercentage,
            PositionYPercentage = gem.PositionYPercentage,
            CreatedOn = gem.CreatedOn
        };
    }

    private static UpdateGemOutputModel MapGemToUpdateGemOutputModel(Gem gem)
    {
        return new UpdateGemOutputModel()
        {
            Id = gem.Id,
            CreatorId = gem.CreatorId,
            CollectionId = gem.CollectionId,
            Name = gem.Name,
            Rarity = gem.Rarity,
            ImageId = gem.ImageId,
            SizePercentage = gem.SizePercentage,
            PublishedStatus = gem.PublishedStatus,
            PositionXPercentage = gem.PositionXPercentage,
            PositionYPercentage = gem.PositionYPercentage,
            CreatedOn = gem.CreatedOn
        };
    }

    private static UpdateStatusGemOutputModel MapGemToUpdateStatusGemOutputModel(Gem gem, bool autoUnpublishedCollection)
    {
        return new UpdateStatusGemOutputModel()
        {
            Id = gem.Id,
            CreatorId = gem.CreatorId,
            CollectionId = gem.CollectionId,
            Name = gem.Name,
            Rarity = gem.Rarity,
            ImageId = gem.ImageId,
            SizePercentage = gem.SizePercentage,
            PublishedStatus = gem.PublishedStatus,
            PositionXPercentage = gem.PositionXPercentage,
            PositionYPercentage = gem.PositionYPercentage,
            CreatedOn = gem.CreatedOn,
            AutoUnpublishedCollection = autoUnpublishedCollection
        };
    }

    private static CreateGemOutputModel MapGemToCreateGemOutputModel(Gem gem)
    {
        return new CreateGemOutputModel()
        {
            Id = gem.Id,
            CreatorId = gem.CreatorId,
            CollectionId = gem.CollectionId,
            Name = gem.Name,
            Rarity = gem.Rarity,
            ImageId = gem.ImageId,
            SizePercentage = gem.SizePercentage,
            PublishedStatus = gem.PublishedStatus,
            PositionXPercentage = gem.PositionXPercentage,
            PositionYPercentage = gem.PositionYPercentage,
            CreatedOn = gem.CreatedOn
        };
    }

    private static FetchGemOutputModel MapGemToFetchGemOutputModel(Gem gem)
    {
        return new FetchGemOutputModel
        {
            Id = gem.Id,
            CreatorId = gem.CreatorId,
            CollectionId = gem.CollectionId,
            Name = gem.Name,
            Rarity = gem.Rarity,
            ImageId = gem.ImageId,
            SizePercentage = gem.SizePercentage,
            PublishedStatus = gem.PublishedStatus,
            PositionXPercentage = gem.PositionXPercentage,
            PositionYPercentage = gem.PositionYPercentage,
            CreatedOn = gem.CreatedOn
        };
    }

    private async Task<bool> CheckCollectionIsForThisCreator(string collectionId, string creatorId)
    {
        _logger.LogDebug("Entered CheckCollectionIsForThisCreator");
        _logger.LogInformation($"Checking Collection Id: {collectionId} | creatorId: {creatorId}");

        var collection = await _collectionRepo.GetAsync(collectionId);

        return collection?.CreatorId == creatorId;
    }


    private async Task<bool> ValidateImageExistsAndOwnedByCreator(string imageId, string creatorId)
    {
        _logger.LogDebug("Entered ValidateImageExistsAndOwnedByCreator");
        _logger.LogInformation($"Checking Image Id: {imageId} | creatorId: {creatorId}");
        
        var image = await _imageRepo.FetchImageRecordByImageIdAndCreatorId(imageId, creatorId);
        if (image is null)
        {
            _logger.LogWarning("Banner Image either doesn't exist or doesn't belong to this creator");
            return false;
        }

        _logger.LogDebug("Image found for this Creator.");
        return true;
    }
}