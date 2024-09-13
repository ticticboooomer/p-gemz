using System.Text.RegularExpressions;
using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Creator.Service.Creator;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepo;
    private readonly IImageRepository _imageRepo;
    private readonly IStoreTagRepository _storeTagRepo;
    private readonly ILogger<StoreService> _logger;

    public StoreService(IStoreRepository storeRepo, IImageRepository imageRepo, IStoreTagRepository storeTagRepo,
        ILogger<StoreService> logger)
    {
        _storeRepo = storeRepo;
        _imageRepo = imageRepo;
        _storeTagRepo = storeTagRepo;
        _logger = logger;
    }

    public async Task<GenericResponse<StoreModel>> EditOrInsertStoreDetails(StoreUpsertModel storeUpsertModel,
        string creatorId)
    {
        _logger.LogDebug("Entered EditOrInsertStoreDetails function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000500"
            };
        }

        if (storeUpsertModel.MaxAutoOpenRarity is < 0 or > 4)
        {
            _logger.LogError("MaxAutoOpenRarity field was set to an invalid number. Leaving function.");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000504"
            };
        }

        if (string.IsNullOrEmpty(storeUpsertModel.Name) || storeUpsertModel.Name.Length > 100)
        {
            _logger.LogError("Store Name field was empty or > 100 chars. Leaving function.");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000505"
            };
        }

        if (storeUpsertModel.UrlStoreTag.Length > 30)
        {
            _logger.LogError("Url Store Tag is too long. Leaving Function.");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000506"
            };
        }

        if (storeUpsertModel.UrlStoreTag.Length < 3)
        {
            _logger.LogError("Url Store Tag is too short. Leaving Function.");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000510"
            };
        }
        
        if (!CheckTagWordCharacterMatch(storeUpsertModel.UrlStoreTag))
        {
            _logger.LogError("Url Store Tag contains invalid characters. Leaving Function.");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000509"
            };
        }
        

        _logger.LogInformation($"Fetching data for store with Id {storeUpsertModel.Id} and Creator Id {creatorId}");

        var existingStore = await _storeRepo.GetByCreatorIdAsync(creatorId);

        if (existingStore is not null)
        {
            if (existingStore.CreatorId != creatorId)
            {
                _logger.LogError("Creator Id for existing store does note match Creator Id passed in");
                return new GenericResponse<StoreModel>()
                {
                    Error = "CR000501"
                };
            }
        }

        if (!string.IsNullOrEmpty(storeUpsertModel.BannerImageId) || string.IsNullOrEmpty(storeUpsertModel.LogoImageId))
        {
            var imagesExistAndOwnedByCreator = await ValidateImagesOwnedByCreator(storeUpsertModel.BannerImageId,
                storeUpsertModel.LogoImageId, creatorId);
            if (!imagesExistAndOwnedByCreator)
            {
                _logger.LogError("Banner and/or Logo images either don't exist for this creator");
                return new GenericResponse<StoreModel>()
                {
                    Error = "CR000503"
                };
            }
        }

        if (existingStore is null ||
            (existingStore is not null && !string.Equals(existingStore.UrlStoreTag, storeUpsertModel.UrlStoreTag, StringComparison.OrdinalIgnoreCase)))
        {
            var errorResponse = await DoStoreTagManipulations(existingStore, storeUpsertModel, creatorId);
            if (!string.IsNullOrEmpty(errorResponse))
            {
                _logger.LogError("Problem during manipulation of StoreTag. Exiting.");
                return new GenericResponse<StoreModel>()
                {
                    Error = errorResponse
                };
            }
        }

        Store updatedStore;
        if (existingStore is null)
        {
            _logger.LogDebug("Building new Store record for Creator");
            var storeDetails = await BuildNewStoreRecord(storeUpsertModel, creatorId);
            _logger.LogInformation($"CreatorId: {creatorId} | StoreId: {storeDetails.Id}");
            updatedStore = await _storeRepo.CreateAsync(storeDetails);

        }
        else
        {
            _logger.LogDebug("Building new Store record for Creator");
            var storeDetails = await BuildAmendedStoreRecord(existingStore, storeUpsertModel);
            _logger.LogInformation($"CreatorId: {creatorId} | StoreId: {storeDetails.Id}");
            updatedStore = await _storeRepo.UpdateAsync(storeDetails);
        }

        if (updatedStore is null)
        {
            _logger.LogWarning("Problem during UpsertAsync operation of stores in database");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000502"
            };
        }

        var storeReturnData = MapStoreToStoreModel(updatedStore);
        return new GenericResponse<StoreModel>()
        {
            Data = storeReturnData
        };
    }

    public async Task<GenericResponse<StoreModel>> FetchCreatorStoreDetails(string creatorId)
    {
        _logger.LogDebug("Entered FetchCreatorStoreDetails function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("No CreatorId Detected. Leaving function.");
            return new GenericResponse<StoreModel>()
            {
                Error = "CR000400"
            };
        }

        _logger.LogInformation($"Fetching Store data for Creator Id {creatorId}");

        var store = await _storeRepo.GetByCreatorIdAsync(creatorId);

        if (store == null)
        {
            return new GenericResponse<StoreModel>()
            {
                Data = new StoreModel()
                {
                    Name = string.Empty,
                    LiveDate = DateTime.MaxValue
                }
            };
        }

        var storeData = MapStoreToStoreModel(store);
        return new GenericResponse<StoreModel>()
        {
            Data = storeData
        };
    }

    public async Task<GenericResponse<TagWordValidityModel>> CheckTagWordAvailable(string creatorId, TagWordModel tagWordModel)
    {
        _logger.LogDebug("Entered CheckTagWordAvailable function");

        if (string.IsNullOrEmpty(creatorId))
        {
            _logger.LogError("Creator Id missing. Exiting.");
            return new GenericResponse<TagWordValidityModel>()
            {
                Error = "CR000900"
            };
        }

        if (string.IsNullOrEmpty(tagWordModel.TagWord))
        {
            _logger.LogError("Tag Word is missing. Exiting.");
            return new GenericResponse<TagWordValidityModel>()
            {
                Error = "CR000901"
            };
        }

        if (tagWordModel.TagWord.Length > 30)
        {
            _logger.LogError("Tag Word is too long, therefore invalid. Leaving Function.");
            return new GenericResponse<TagWordValidityModel>()
            {
                Error = "CR000902"
            };
        }

        if (tagWordModel.TagWord.Length < 3)
        {
            _logger.LogError("Url Store Tag is too short. Leaving Function.");
            return new GenericResponse<TagWordValidityModel>()
            {
                Error = "CR000904"
            };
        }

        
        if (!CheckTagWordCharacterMatch(tagWordModel.TagWord))
        {
            _logger.LogError("Tag Word contains invalid characters. Leaving Function.");
            return new GenericResponse<TagWordValidityModel>()
            {
                Error = "CR000903"
            };
        }


        var existingStoreTag = await _storeTagRepo.GetAsync(tagWordModel.TagWord.ToLower());

        if (existingStoreTag is null)
        {
            _logger.LogDebug("Tag word does not exist in Store Tags so is available to use");
            return new GenericResponse<TagWordValidityModel>()
            {
                Data = new TagWordValidityModel()
                {
                    TagWordAvailable = true
                }
            };
        }

        if (existingStoreTag.CreatorId == creatorId)
        {
            _logger.LogDebug("Tag word currently in use by this creator so is available to use");
            return new GenericResponse<TagWordValidityModel>()
            {
                Data = new TagWordValidityModel()
                {
                    TagWordAvailable = true
                }
            };
        }

        _logger.LogDebug(
            "Tag word does exist in Store Tags but for a different creator so is NOT available to use");
        return new GenericResponse<TagWordValidityModel>()
        {
            Data = new TagWordValidityModel()
            {
                TagWordAvailable = false
            }
        };
    }

    private  static bool CheckTagWordCharacterMatch(string tagWord)
    {
        const string matchExpression = @"^[a-zA-Z0-9]*$";
        var match = Regex.Match(tagWord, matchExpression, RegexOptions.IgnoreCase);

        return match.Success;
    }

    private async Task<string> DoStoreTagManipulations(Store existingStore, StoreUpsertModel storeUpsertModel,
        string creatorId)
    {
        var response = await ValidateStoreTagToBeUsed(storeUpsertModel.UrlStoreTag, creatorId);

        if (!string.IsNullOrEmpty(response))
            return response;

        var errorOnCreate = await CreateStoreTagRecord(storeUpsertModel.UrlStoreTag, creatorId);
        if (!string.IsNullOrEmpty(errorOnCreate))
        {
            _logger.LogError("Problem creating the StoreTag. Cannot proceed.");
            return "CR000508";
        }

        if (existingStore is null)
        {
            return string.Empty;
        }

        // Fetch creators current tag word
        var creatorOriginalTag = await _storeTagRepo.GetAsync(existingStore.UrlStoreTag.ToLower());
        if (creatorOriginalTag is not null)
        {
            var resp = await _storeTagRepo.DeleteAsync(creatorOriginalTag.Tagword.ToLower());
        }

        return string.Empty;
    }

    private async Task<string> ValidateStoreTagToBeUsed(string newStoreTag, string creatorId)
    {
        var existingStoreTag = await _storeTagRepo.GetAsync(newStoreTag.ToLower());
        if (existingStoreTag is null) return string.Empty;

        if (existingStoreTag.CreatorId == creatorId) return string.Empty;
        
        _logger.LogError("Tag Word for store URL already in use. Cannot Proceed.");
        return "CR000507";
    }

    private async Task<string> CreateStoreTagRecord(string tagWord, string creatorId)
    {
        if (string.IsNullOrEmpty(tagWord) || string.IsNullOrEmpty(creatorId))
        {
            return "CR000508";
        }

        var newStoreTag = await _storeTagRepo.CreateAsync(new StoreTag()
        {
            Id = tagWord.ToLower(),
            Tagword = tagWord.ToLower(),
            CreatorId = creatorId,
            CreatedOn = DateTime.UtcNow
        });

        return newStoreTag is null ? "CR000508" : string.Empty;
    }

    private static StoreModel MapStoreToStoreModel(Store store)
    {
        return new StoreModel()
        {
            Id = store.Id,
            Name = store.Name,
            BannerImageId = store.BannerImageId,
            LogoImageId = store.LogoImageId,
            MaxAutoOpenRarity = store.MaxAutoOpenRarity,
            LiveDate = store.LiveDate,
            UrlStoreTag = store.UrlStoreTag
        };
    }

    private async Task<bool> ValidateImagesOwnedByCreator(string? storeBannerImageId, string? storeLogoImageId,
        string creatorId)
    {
        if (!string.IsNullOrEmpty(storeBannerImageId))
        {
            _logger.LogDebug("Validating BannerImage ownership");
            var image = await _imageRepo.FetchImageRecordByImageIdAndCreatorId(storeBannerImageId, creatorId);
            if (image is null)
            {
                _logger.LogWarning("Banner Image either doesn't exist or doesn't belong to this creator");
                return false;
            }
        }

        if (!string.IsNullOrEmpty(storeLogoImageId))
        {
            _logger.LogDebug("Validating LogoImage ownership");
            var image = await _imageRepo.FetchImageRecordByImageIdAndCreatorId(storeLogoImageId, creatorId);
            if (image is null)
            {
                _logger.LogWarning("Logo Image either doesn't exist or doesn't belong to this creator");
                return false;
            }
        }

        return true;
    }

    private async Task<Store> BuildNewStoreRecord(StoreUpsertModel storeUpsertModel, string creatorId)
    {
        return new Store()
        {
            Id = Guid.NewGuid().ToString(),
            CreatorId = creatorId,
            Name = storeUpsertModel.Name,
            BannerImageId = storeUpsertModel.BannerImageId,
            LogoImageId = storeUpsertModel.LogoImageId,
            MaxAutoOpenRarity = storeUpsertModel.MaxAutoOpenRarity,
            CreatedOn = DateTime.UtcNow,
            LiveDate = storeUpsertModel.LiveDate ?? DateTime.MaxValue,
            UrlStoreTag = storeUpsertModel.UrlStoreTag
        };
    }

    private async Task<Store> BuildAmendedStoreRecord(Store existingStore, StoreUpsertModel storeUpsertModel)
    {
        var liveDateToUse = DateTime.MaxValue;
        if (storeUpsertModel.LiveDate != null)
        {
            liveDateToUse = (DateTime)storeUpsertModel.LiveDate;
        }
        else if (existingStore.LiveDate != null)
        {
            liveDateToUse = existingStore.LiveDate;
        }

        return new Store()
        {
            Id = existingStore.Id,
            CreatorId = existingStore.CreatorId,
            Name = storeUpsertModel.Name,
            BannerImageId = storeUpsertModel.BannerImageId,
            LogoImageId = storeUpsertModel.LogoImageId,
            MaxAutoOpenRarity = storeUpsertModel.MaxAutoOpenRarity,
            LiveDate = liveDateToUse,
            UrlStoreTag = storeUpsertModel.UrlStoreTag,
            CreatedOn = existingStore.CreatedOn
        };
    }
}