using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Collector.Service.Collector;

public class BasketService : IBasketService
{
    private readonly IBasketRepository _basketRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IStoreTagRepository _storeTagRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ILogger<BasketService> _logger;

    public BasketService(IBasketRepository basketRepo,
        ICollectionRepository collectionRepo,
        IStoreRepository storeRepo,
        IStoreTagRepository storeTagRepo,
        ILogger<BasketService> logger, IAccountRepository accountRepo)
    {
        _basketRepo = basketRepo;
        _collectionRepo = collectionRepo;
        _storeRepo = storeRepo;
        _logger = logger;
        _accountRepo = accountRepo;
        _storeTagRepo = storeTagRepo;
    }

    public async Task<GenericResponse<BasketModel>> GetActiveBasketForCollectorById(string collectorId, BasketGetByIdInputModel basketGetByIdInputModel)
    {
        _logger.LogDebug("Entered GetActiveBasketForCollectorById function");
        _logger.LogInformation($"CollectorId: {collectorId}");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Missing CollectorId. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000200"
            };
        }

        if (string.IsNullOrEmpty(basketGetByIdInputModel?.BasketId))
        {
            _logger.LogError("Missing or empty BasketId. Leaving Function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000202"
            };
        }

        var returnedBasket = await _basketRepo.GetActiveBasketById(basketGetByIdInputModel.BasketId);

        if (returnedBasket is null)
        {
            _logger.LogError("Repo error fetching basket. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000201"
            };
        }

        if (returnedBasket.CollectorId != collectorId)
        {
            _logger.LogError("Basket is not for this collector. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000203"
            };
        }

        var completeBasket = await BuildOutputBasketModelWithStoreDetails(returnedBasket);
        if (completeBasket is null)
        {
            _logger.LogError("Repo error on fetching Store details");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000204"
            };
        }

        return new GenericResponse<BasketModel>()
        {
            Data = completeBasket
        };
    }

    public async Task<GenericResponse<BasketModel>> GetActiveBasketForCollectorStoreTag(string collectorId,
        BasketGetByStoreTagInputModel basketGetByStoreTagInputModel)
    {
        _logger.LogDebug("Entered GetActiveBasketForCollectorStoreTag function");
        _logger.LogInformation($"CollectorId: {collectorId}");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Missing or Empty Collector Id parameter. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL003300"
            };
        }

        if (string.IsNullOrEmpty(basketGetByStoreTagInputModel?.StoreTag))
        {
            _logger.LogError("Missing or Empty Store Tag parameter. Leaving Function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL003301"
            };
        }

        var storeTagDetails = await _storeTagRepo.GetAsync(basketGetByStoreTagInputModel.StoreTag);
        if (storeTagDetails is null)
        {
            _logger.LogError("Repo error fetching StoreTag details. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL003302"
            };
        }

        var returnedBasket = await _basketRepo.GetCreatorBasketForCollector(collectorId, storeTagDetails.CreatorId);
        if (returnedBasket is null)
        {
            _logger.LogInformation("No basket exists for this collector in this store");
            return new GenericResponse<BasketModel>()
            {
                Data = new BasketModel()
            };
        }

        var completeBasket = await BuildOutputBasketModelWithStoreDetails(returnedBasket);
        if (completeBasket is null)
        {
            _logger.LogError("Repo error on fetching Store details. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL003303"
            };
        }

        return new GenericResponse<BasketModel>()
        {
            Data = completeBasket
        };
    }

    public async Task<GenericResponse<BasketModel>> AddItemToBasket(string collectorId,
        BasketItemAddModel basketItemAddModel)
    {
        _logger.LogDebug("Entered AddItemToBasket function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("CollectorId is missing or empty. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000300"
            };
        }

        if (basketItemAddModel is null)
        {
            _logger.LogError("Problem with BasketItemAddModel. It is null. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000301"
            };
        }

        if (string.IsNullOrEmpty(basketItemAddModel.CollectionId))
        {
            _logger.LogError("CollectionId was null or empty. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000302"
            };
        }

        if (basketItemAddModel.Quantity <= 0)
        {
            _logger.LogError("Invalid line item Quantity passed in (<=0). Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000303"
            };
        }

        var existingCollection = await
            _collectionRepo.GetSingleCollection(basketItemAddModel.CollectionId);

        if (existingCollection is null)
        {
            _logger.LogError("CollectionId for line item does not exist in Database. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000304"
            };
        }

        var existingBasketForCreatorStore =
            await _basketRepo.GetCreatorBasketForCollector(collectorId, existingCollection.CreatorId);
        if (existingBasketForCreatorStore is not null)
        {
            return await AddLineToExistingBasket(existingBasketForCreatorStore, existingCollection, basketItemAddModel.Quantity);
        }

        _logger.LogDebug("Basket does not already exist for Collector. Building new Basket.");
        return await BuildNewBasketWithLineAdded(collectorId, existingCollection, basketItemAddModel);
    }

    public async Task<GenericResponse<BasketModel>> RemoveItemFromBasket(string collectorId, BasketItemRemoveModel basketItemRemoveModel)
    {
        _logger.LogDebug("Entered RemoveItemFromBasket function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("CollectorId is missing. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000400"
            };
        }

        if (basketItemRemoveModel is null)
        {
            _logger.LogWarning("Problem with BasketItemRemoveModel. It is null. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000401"
            };
        }

        if (string.IsNullOrEmpty(basketItemRemoveModel.LineId))
        {
            _logger.LogError("LineId parameter was null or empty. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000402"
            };
        }

        if (string.IsNullOrEmpty(basketItemRemoveModel.BasketId))
        {
            _logger.LogError("BasketId parameter was null or empty. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000406"
            };
        }

        var collectorBasket = await _basketRepo.GetActiveBasketById(basketItemRemoveModel.BasketId);

        if (collectorBasket is null)
        {
            _logger.LogWarning("Basket does not exist for Collector. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000403"
            };
        }

        _logger.LogDebug("Fetched existing Basket for Collector.");

        var basketLineToRemove = collectorBasket.Items.SingleOrDefault(r => r.Id == basketItemRemoveModel.LineId);
        if (basketLineToRemove is null)
        {
            _logger.LogError("Line to be removed does not exist in Basket for collector. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000404"
            };
        }

        _logger.LogDebug("Found line to be removed.");
        collectorBasket.Items.Remove(basketLineToRemove);
        if (collectorBasket.Items.Count == 0)
        {
            _logger.LogDebug("Last item removed from basket so making basket inactive.");
            collectorBasket.Active = false;
        }

        var updatedBasket = await _basketRepo.UpdateBasketAsync(collectorBasket);
        _logger.LogDebug("Returned from UpdateBasketItemForCollector repo function.");

        if (updatedBasket is null)
        {
            _logger.LogError("Repo error during Update action of Basket. Leaving function");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000405"
            };
        }

        _logger.LogDebug("Basket Update successful.");
        if (updatedBasket.Active == false)
        {
            _logger.LogDebug("No active basket so returning empty basket.");
            return new GenericResponse<BasketModel>()
            {
                Data = new BasketModel()
            };
        }

        var builtBasket = await BuildOutputBasketModelWithStoreDetails(updatedBasket);
        if (builtBasket is null)
        {
            _logger.LogError("Repo error on fetching Store details. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000407"
            };
        }

        return new GenericResponse<BasketModel>()
        {
            Data = builtBasket
        };
    }

    public async Task<GenericResponse<BasketModel>> UpdateItemInBasket(string collectorId,
        BasketItemUpdateModel basketItemUpdateModel)
    {
        _logger.LogDebug("Entered UpdateItemInBasket function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("CollectorId is missing. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000500"
            };
        }

        if (basketItemUpdateModel is null)
        {
            _logger.LogError("Problem with BasketItemUpdateModel. It is null. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000501"
            };
        }

        if (string.IsNullOrEmpty(basketItemUpdateModel.LineId))
        {
            _logger.LogError("LineId was null or empty. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000502"
            };
        }

        if (basketItemUpdateModel.Quantity <= 0)
        {
            _logger.LogError("Quantity parameter had invalid value (<=0). Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000503"
            };
        }

        if (string.IsNullOrEmpty(basketItemUpdateModel.BasketId))
        {
            _logger.LogError("Missing or Empty BasketId parameter. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000507"
            };
        }

        var collectorBasket = await _basketRepo.GetActiveBasketById(basketItemUpdateModel.BasketId);
        if (collectorBasket is null)
        {
            _logger.LogError("Basket does not exist for Collector. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000504"
            };
        }

        _logger.LogDebug("Fetched existing Basket for Collector.");

        var lineIndex = collectorBasket.Items.FindIndex(i => i.Id == basketItemUpdateModel.LineId);
        if (lineIndex == -1)
        {
            _logger.LogError("Line Id for update does not exist in collector basket");
            _logger.LogInformation($"LineId passed in: {basketItemUpdateModel.LineId}");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000505"
            };
        }

        _logger.LogDebug("Found basket line to update");

        collectorBasket.Items[lineIndex].Quantity = basketItemUpdateModel.Quantity;

        var updatedBasket = await _basketRepo.UpdateBasketAsync(collectorBasket);
        if (updatedBasket is null)
        {
            _logger.LogError("Repo error during Update action of Basket. Leaving function");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000506"
            };
        }

        _logger.LogDebug("Basket Update successful. Building BasketModel for returning.");
        var builtBasket = await BuildOutputBasketModelWithStoreDetails(updatedBasket);
        if (builtBasket is not null)
            return new GenericResponse<BasketModel>()
            {
                Data = builtBasket
            };

        _logger.LogError("Repo error on fetching Store details. Leaving function.");
        return new GenericResponse<BasketModel>()
        {
            Error = "CL000508"
        };

    }

    public async Task<GenericResponse<BasketModel>> EmptyBasket(string collectorId, EmptyBasketInputModel emptyBasketInputModel)
    {
        _logger.LogDebug("Entered EmptyBasket function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("CollectorId is missing. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000600"
            };
        }

        if (string.IsNullOrEmpty(emptyBasketInputModel.BasketId))
        {
            _logger.LogError("Empty or missing BasketId parameter. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000603"
            };
        }

        var collectorBasket = await _basketRepo.GetActiveBasketById(emptyBasketInputModel.BasketId);

        if (collectorBasket is null)
        {
            _logger.LogError("Basket does not exist for Collector. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000601"
            };
        }

        _logger.LogDebug("Fetched existing Basket for Collector.");

        collectorBasket.Items.Clear();
        collectorBasket.Active = false;

        var updatedBasket = await _basketRepo.UpdateBasketAsync(collectorBasket);
        if (updatedBasket is null)
        {
            _logger.LogError("Repo error during Update action of Basket. Leaving function");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000602"
            };
        }

        _logger.LogDebug("Basket Update successful.");
        return new GenericResponse<BasketModel>()
        {
            Data = new BasketModel()
        };
    }


    public async Task<GenericResponse<List<BasketIdOutputModel>>> GetAllActiveBasketsForOneCollector(string collectorId)
    {
        _logger.LogDebug("Entered GetAllBasketsForOneCollector function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Missing or Empty Collector Id parameter. Leaving function.");
            return new GenericResponse<List<BasketIdOutputModel>>()
            {
                Error = "CL003200"
            };
        }

        var basketList = await _basketRepo.GetAllBasketsForCollector(collectorId);
        if (basketList is null)
        {
            _logger.LogError("Repo error during fetch of collector baskets. Leaving function.");
            return new GenericResponse<List<BasketIdOutputModel>>()
            {
                Error = "CL003201"
            };
        }

        if (basketList.Count == 0)
        {
            _logger.LogDebug("No active baskets for this collector. Returning empty list.");
            return new GenericResponse<List<BasketIdOutputModel>>()
            {
                Data = new List<BasketIdOutputModel>()
            };
        }

        _logger.LogDebug("Building list of basket models for collector");

        var basketIdModelList = new List<BasketIdOutputModel>();

        foreach (var basket in basketList)
        {
            basketIdModelList.Add(new BasketIdOutputModel()
            {
                BasketId = basket.Id
            });
        }

        return new GenericResponse<List<BasketIdOutputModel>>()
        {
            Data = basketIdModelList
        };
    }

    public async Task<GenericResponse<BasketItemOutputModel>> GetBasketItemData(BasketItemInputModel basketItemInputModel)
    {
        _logger.LogDebug("Entered GetBasketItemData function.");

        if (string.IsNullOrEmpty(basketItemInputModel.CollectionId))
        {
            _logger.LogError("Missing or empty collectionId parameter. Leaving function.");
            return new GenericResponse<BasketItemOutputModel>()
            {
                Error = "CL003400"
            };
        }

        var collection = await _collectionRepo.GetSingleCollection(basketItemInputModel.CollectionId);
        if (collection is null)
        {
            _logger.LogError($"Repo error during fetch of collectionId {basketItemInputModel.CollectionId}");
            return new GenericResponse<BasketItemOutputModel>()
            {
                Error = "CL003401"
            };
        }

        return new GenericResponse<BasketItemOutputModel>()
        {
            Data = new BasketItemOutputModel()
            {
                CollectionId = collection.Id,
                CollectionName = collection.Name,
                PackPrice = collection.ClusterPrice
            }
        };
    }

    public async Task<GenericResponse<BasketPagedOutputModel>> GetAllActiveBasketsForOneCollectorPaged(
        string collectorId,
        GetAllBasketsPageInputModel getAllBasketsPageInputModel)
    {
        _logger.LogDebug("Entered GetAllActiveBasketsForOneCollectorPaged function.");

        if (string.IsNullOrEmpty(collectorId))
        {
            _logger.LogError("Missing or empty collectorId parameter. Leaving function.");
            return new GenericResponse<BasketPagedOutputModel>()
            {
                Error = "CL003500"
            };
        }

        if (getAllBasketsPageInputModel.CurrentPage < 0)
        {
            _logger.LogError("CurrentPage parameter is < 0 which is invalid. Leaving function.");
            return new GenericResponse<BasketPagedOutputModel>()
            {
                Error = "CL003501"
            };
        }

        if (getAllBasketsPageInputModel.PageSize <= 0)
        {
            _logger.LogError("PageSize parameter is <= 0 which is invalid. Leaving function.");
            return new GenericResponse<BasketPagedOutputModel>()
            {
                Error = "CL003502"
            };
        }

        var creatorToExclude = string.Empty;
        if (!string.IsNullOrEmpty(getAllBasketsPageInputModel.StoreTagToExclude))
        {
            var storeTagDetails = await _storeTagRepo.GetAsync(getAllBasketsPageInputModel.StoreTagToExclude);
            if (storeTagDetails is null)
            {
                _logger.LogError("StoreTagToExclude is not a valid store tag. Leaving function.");
                return new GenericResponse<BasketPagedOutputModel>()
                {
                    Error = "CL003503"
                };
            }

            creatorToExclude = storeTagDetails.CreatorId;
        }


        var basketPage = await _basketRepo.GetPageOfBasketsForCollector(collectorId, 
                                                                        getAllBasketsPageInputModel.CurrentPage, 
                                                                        getAllBasketsPageInputModel.PageSize, 
                                                                        creatorToExclude);
        if (basketPage is null)
        {
            _logger.LogError("Repo error during fetch of Basket Page. Leaving function.");
            return new GenericResponse<BasketPagedOutputModel>()
            {
                Error = "CL003504"
            };
        }

        _logger.LogInformation($"Repo returned {basketPage.Baskets.Count} basket records.");
        var basketsPagedModel = new BasketPagedOutputModel()
        {
            Baskets = new List<BasketIdOutputModel>(),
            ThisPage = basketPage.ThisPage,
            TotalPages = basketPage.TotalPages
        };

        _logger.LogDebug("Building BasketPagedOutputModel return type from repo data.");
        foreach (var basket in basketPage.Baskets)
        {
            basketsPagedModel.Baskets.Add(new BasketIdOutputModel()
            {
                BasketId = basket.Id
            });
        }

        _logger.LogDebug("Return data built, returning data.");
        return new GenericResponse<BasketPagedOutputModel>()
        {
            Data = basketsPagedModel
        };
    }

    public async Task<GenericResponse<StripeAccountOutputModel>> GetPaymentAccountByBasketId(
        PaymentAccountInputModel paymentAccountInputModel)
    {
        _logger.LogDebug("Entered GetPaymentAccountByBasketId function.");

        if (string.IsNullOrEmpty(paymentAccountInputModel.BasketId))
        {
            _logger.LogError("Missing or empty BasketId parameter. Leaving function.");
            return new GenericResponse<StripeAccountOutputModel>()
            {
                Error = "CL003700"
            };
        }

        var basket = await _basketRepo.GetAnyStatusBasketById(paymentAccountInputModel.BasketId);
        if (basket is null)
        {
            _logger.LogError("Repo error during fetch of basket. Leaving function.");
            return new GenericResponse<StripeAccountOutputModel>()
            {
                Error = "CL003701"
            };
        }

        if (string.IsNullOrEmpty(basket.Id))
        {
            _logger.LogError("Basket not found in Repo. Leaving function.");
            return new GenericResponse<StripeAccountOutputModel>()
            {
                Error = "CL003702"
            };
        }

        if (string.IsNullOrEmpty(basket.CreatorId))
        {
            _logger.LogError("Basket has missing CreatorId. Leaving function.");
            return new GenericResponse<StripeAccountOutputModel>()
            {
                Error = "CL003703"
            };
        }

        var creatorDetails = await _accountRepo.GetAccountById(basket.CreatorId);
        if (creatorDetails is null)
        {
            _logger.LogError("Repo Error during fetch of Creator Details. Leaving Function.");
            return new GenericResponse<StripeAccountOutputModel>()
            {
                Error = "CL003704"
            };
        }

        if (string.IsNullOrEmpty(creatorDetails.StripeAccountId))
        {
            _logger.LogError("Creator for this basket as null or empty Stripe Account. Leaving function.");
            return new GenericResponse<StripeAccountOutputModel>()
            {
                Error = "CL003705"
            };
        }
        return new GenericResponse<StripeAccountOutputModel>()
        {
            Data = new StripeAccountOutputModel()
            {
                StripePaymentAccount = creatorDetails.StripeAccountId
            }
        };
    }



    private BasketItem BuildBasketItem(string collectionId, int quantity)
    {
        return new BasketItem()
        {
            Id = Guid.NewGuid().ToString(),
            CollectionId = collectionId,
            Quantity = quantity
        };
    }

    private async Task<BasketModel> BuildOutputBasketModelWithStoreDetails(Basket basket)
    {
        _logger.LogDebug("Entered BuildOutputBasketModelWithStoreDetails function.");

        var storeDetails = await _storeRepo.GetByCreatorId(basket.CreatorId);
        if (storeDetails is null)
        {
            _logger.LogError($"Repo Error during fetch of Store details for creator {basket.CreatorId} when building output model. Leaving function.");
            return null;
        }

        var basketModel = new BasketModel()
        {
            BasketId = basket.Id,
            StoreTag = storeDetails.UrlStoreTag,
            StoreName = storeDetails.Name,
            StoreLogoImageId = storeDetails.LogoImageId,
            Items = new List<BasketItemModel>()
        };

        foreach (var basketItemModel in basket.Items.Select(item => new BasketItemModel
        {
            LineId = item.Id,
            CollectionId = item.CollectionId,
            Quantity = item.Quantity,
        }))
        {
            basketModel.Items.Add(basketItemModel);
        }

        return basketModel;
    }

    private async Task<GenericResponse<BasketModel>> BuildNewBasketWithLineAdded(string collectorId, Collection existingCollection, BasketItemAddModel basketItemAddModel)
    {
        var newBasket = new Basket
        {
            Id = Guid.NewGuid().ToString(),
            CollectorId = collectorId,
            CreatorId = existingCollection.CreatorId,
            Active = true,
            CreatedOn = DateTime.Now,
            Items = new List<BasketItem>()
            {
                BuildBasketItem(existingCollection.Id, basketItemAddModel.Quantity)
            }
        };

        _logger.LogDebug("Adding new basket with line item.");
        var addedBasket = await _basketRepo.CreateBasketAsync(newBasket);
        if (addedBasket is null)
        {
            _logger.LogError("Repo error during create of new basket with line item added. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000305"
            };
        }

        var basketOutputModel = await BuildOutputBasketModelWithStoreDetails(addedBasket);
        if (basketOutputModel is null)
        {
            _logger.LogError("Repo error during create of new basket with line item added. Leaving function.");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000307"
            };
        }

        return new GenericResponse<BasketModel>()
        {
            Data = basketOutputModel
        };
    }

    private async Task<GenericResponse<BasketModel>> AddLineToExistingBasket(Basket existingBasket, Collection existingCollection, int quantity)
    {

        var indexCollectionInBasket =
            existingBasket.Items.FindIndex(x => x.CollectionId == existingCollection.Id);
        if (indexCollectionInBasket >= 0)
        {
            _logger.LogDebug("Collection already in basket. Increasing Qty.");
            existingBasket.Items[indexCollectionInBasket].Quantity += quantity;
        }
        else
        {
            _logger.LogDebug("Collection not in basket so adding line for it.");
            var newBasketItem = BuildBasketItem(existingCollection.Id, quantity);
            existingBasket.Items.Add(newBasketItem);
        }

        var updatedBasket = await _basketRepo.UpdateBasketAsync(existingBasket);
        _logger.LogDebug("Returned from UpdateBasketAsync repo function.");


        if (updatedBasket is null)
        {
            _logger.LogError("Repo error during Update action of Basket. Leaving function");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000306"
            };
        }

        _logger.LogDebug("Basket Update successful. Building BasketModel for returning.");

        var builtBasket = await BuildOutputBasketModelWithStoreDetails(updatedBasket);
        if (builtBasket is null)
        {
            _logger.LogError("Repo error on fetching Store details");
            return new GenericResponse<BasketModel>()
            {
                Error = "CL000307"
            };
        }

        return new GenericResponse<BasketModel>()
        {
            Data = builtBasket
        };
    }
}
