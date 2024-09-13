using System.Text.RegularExpressions;
using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Collector.Service.Collector;

public class StoreService : IStoreService
{
    private readonly IStoreTagRepository _storeTagRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly ILogger<StoreService> _logger;

    public StoreService(IStoreTagRepository storeTagRepo, IStoreRepository storeRepo, ILogger<StoreService> logger)
    {
        _storeTagRepo = storeTagRepo;
        _storeRepo = storeRepo;
        _logger = logger;
    }
    
    public async Task<GenericResponse<StoreValidityModel>> CheckStoreIsValid(StoreTagModel storeTagModel)
    {
        // NOTE: This method will only return Data or True/False rather than Errors as FE handles this differently
        //       Reason for all the pre-tests before accessing Repo is to reduce hits on Repo in case someone is
        //       playing silly people and spamming us with dodgy URLs
        _logger.LogDebug("Entered CheckStoreIsValid function");

        if (string.IsNullOrEmpty(storeTagModel.StoreTag))
        {
            _logger.LogError("Tag Word is missing. Exiting.");
            return new GenericResponse<StoreValidityModel>()
            {
                Data = new StoreValidityModel()
                {
                    StoreValid = false
                }
            };
        }

        if (storeTagModel.StoreTag.Length > 30)
        {
            _logger.LogError("Tag Word is too long, therefore invalid. Leaving Function.");
            return new GenericResponse<StoreValidityModel>()
            {
                Data = new StoreValidityModel()
                {
                    StoreValid = false
                }
            };
        }

        if (storeTagModel.StoreTag.Length < 3)
        {
            _logger.LogError("Url Store Tag is too short. Leaving Function.");
            return new GenericResponse<StoreValidityModel>()
            {
                Data = new StoreValidityModel()
                {
                    StoreValid = false
                }
            };
        }

        
        if (!CheckTagWordCharacterMatch(storeTagModel.StoreTag))
        {
            _logger.LogError("Tag Word contains invalid characters. Leaving Function.");
            return new GenericResponse<StoreValidityModel>()
            {
                Data = new StoreValidityModel()
                {
                    StoreValid = false
                }
            };
        }


        var existingStoreTag = await _storeTagRepo.GetAsync(storeTagModel.StoreTag.ToLower());

        if (existingStoreTag is null)
        {
            _logger.LogDebug("Tag word does not exist in Store Tags.");
            return new GenericResponse<StoreValidityModel>()
            {
                Data = new StoreValidityModel()
                {
                    StoreValid = false
                }
            };
        }

        _logger.LogDebug("Tag word does exist in Store Tags");
        return new GenericResponse<StoreValidityModel>()
        {
            Data = new StoreValidityModel()
            {
                StoreValid = true
            }
        };
    }

    public async Task<GenericResponse<StoreFrontModel>> FetchCreatorStoreFront(StoreTagModel storeTagModel)
    {
        _logger.LogDebug("Entered FetchStoreFront function");

        if (string.IsNullOrEmpty(storeTagModel.StoreTag))
        {
            _logger.LogError("Tag Word is missing. Exiting.");
            return new GenericResponse<StoreFrontModel>()
            {
                Error = "CL000700"
            };
        }

        if (storeTagModel.StoreTag.Length > 30)
        {
            _logger.LogError("Tag Word is too long, therefore invalid. Leaving Function.");
            return new GenericResponse<StoreFrontModel>()
            {
                Error = "CL000701"
            };
        }

        if (storeTagModel.StoreTag.Length < 3)
        {
            _logger.LogError("Url Store Tag is too short. Leaving Function.");
            return new GenericResponse<StoreFrontModel>()
            {
                Error = "CL000702"
            };
        }

        
        if (!CheckTagWordCharacterMatch(storeTagModel.StoreTag))
        {
            _logger.LogError("Tag Word contains invalid characters. Leaving Function.");
            return new GenericResponse<StoreFrontModel>()
            {
                Error = "CL000703"
            };
        }


        var existingStoreTag = await _storeTagRepo.GetAsync(storeTagModel.StoreTag.ToLower());

        if (existingStoreTag is null)
        {
            _logger.LogError("Tag word does not exist in Store Tags.");
            return new GenericResponse<StoreFrontModel>()
            {
                Error = "CL000704"
            };
        }

        if (string.IsNullOrEmpty(existingStoreTag.CreatorId))
        {
            _logger.LogError("No Creator Id held against this store tag");
            return new GenericResponse<StoreFrontModel>()
            {
                Error = "CL000705"
            };
        }
        _logger.LogDebug("Tag word does exist in Store Tags");

        var creatorStore = await _storeRepo.GetByCreatorId(existingStoreTag.CreatorId);

        if (creatorStore is null)
        {
            _logger.LogError("Could not find Store using Creator Id held against this store tag");
            return new GenericResponse<StoreFrontModel>()
            {
                Error = "CL000706"
            };
        }

        return new GenericResponse<StoreFrontModel>()
        {
            Data = MapStoreToStoreFrontModel(creatorStore)
        };
    }

    public async Task<GenericResponse<List<LiveStoresOutputModel>>> FetchLiveStoresList()
    {
        _logger.LogDebug("Entered FetchLiveStoresList function.");

        var storeList = await _storeRepo.GetAllLiveStores();

        if (storeList is null)
        {
            _logger.LogError("Repo error during GetAllLiveStores fetch. Leaving Function.");
            return new GenericResponse<List<LiveStoresOutputModel>>()
            {
                Error = "CL003100"
            };
        }

        return new GenericResponse<List<LiveStoresOutputModel>>()
        {
            Data = MapStoreListToOutputModel(storeList)
        };
    }

    public async Task<GenericResponse<LiveStoresPagedOutputModel>> FetchLiveStoresPage(
        LiveStoresPagedInputModel liveStoresPagedInputModel)
    {
        _logger.LogDebug("Entered FetchLiveStoresPage function.");

        if (liveStoresPagedInputModel.PageSize <= 0)
        {
            _logger.LogError("PageSize parameter is <= 0 which is invalid. Leaving function.");
            return new GenericResponse<LiveStoresPagedOutputModel>()
            {
                Error = "CL003600"
            };
        }

        if (liveStoresPagedInputModel.CurrentPage < 0)
        {
            _logger.LogError("CurrentPage parameter is < 0 which is invalid. Leaving function.");
            return new GenericResponse<LiveStoresPagedOutputModel>()
            {
                Error = "CL003601"
            };
        }

        var liveStoresPage = await _storeRepo.GetLiveStoresPage(liveStoresPagedInputModel.CurrentPage, liveStoresPagedInputModel.PageSize);
        if (liveStoresPage is null)
        {
            _logger.LogError("Repo error on Fetch of Live Stores Page. Leaving function.");
            return new GenericResponse<LiveStoresPagedOutputModel>()
            {
                Error = "CL003602"
            };
        }

        _logger.LogDebug("Building and returning LiveStoresPagedOutputModel");
        return new GenericResponse<LiveStoresPagedOutputModel>()
        {
            Data = new LiveStoresPagedOutputModel()
            {
                Stores = MapStoreListToOutputModel(liveStoresPage.Stores),
                ThisPage = liveStoresPage.ThisPage,
                TotalPages = liveStoresPage.TotalPages
            }
        };
    }

    private static List<LiveStoresOutputModel> MapStoreListToOutputModel(List<Store> storeList)
    {
        var liveStoresList = new List<LiveStoresOutputModel>();

        foreach (var store in storeList) 
        {
            liveStoresList.Add(new LiveStoresOutputModel()
            {
                StoreTag = store.UrlStoreTag,
                Name = store.Name,
                LogoImageId = store.LogoImageId,
                BannerImageId = store.BannerImageId,
            });
        }

        return liveStoresList;
    }

    private static StoreFrontModel MapStoreToStoreFrontModel(Store store)
    {
        return new StoreFrontModel()
        {
            StoreName = store.Name,
            LiveDate = store.LiveDate,
            StoreLogoImageId = store.LogoImageId,
            StoreBannerImageId = store.BannerImageId
        };
    }
    
    private static bool CheckTagWordCharacterMatch(string tagWord)
    {
        const string matchExpression = @"^[a-zA-Z0-9]*$";
        var match = Regex.Match(tagWord, matchExpression, RegexOptions.IgnoreCase);

        return match.Success;
    }
}