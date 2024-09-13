using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using MassTransit.Futures.Contracts;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Gemz.Api.Collector.Service.Collector
{
    public class PurchasesService : IPurchasesService
    {
        private readonly ICollectorGemRepository _collectorGemRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IStoreTagRepository _storeTagRepo;
        private readonly ICollectionRepository _collectionRepo;
        private readonly IGemRepository _gemRepo;
        private readonly ILogger<PurchasesService> _logger;

        public PurchasesService(ICollectorGemRepository collectorGemRepo,
                                IStoreRepository storeRepo,
                                IStoreTagRepository storeTagRepo,
                                ICollectionRepository collectionRepo,
                                IGemRepository gemRepo,
                                ILogger<PurchasesService> logger)
        {
            _collectorGemRepo = collectorGemRepo;
            _storeRepo = storeRepo;
            _storeTagRepo = storeTagRepo;
            _collectionRepo = collectionRepo;
            _gemRepo = gemRepo;
            _logger = logger;
        }

        public async Task<GenericResponse<List<StoresPurchasedFromOutputModel>>> FetchStoresContainingPurchases(string collectorId)
        {
            _logger.LogDebug("Entered FetchStoresContainingPurchases function");

            if (string.IsNullOrWhiteSpace(collectorId))
            {
                _logger.LogError("Missing or empty collector id parameter.  Leaving Function.");
                return new GenericResponse<List<StoresPurchasedFromOutputModel>>
                {
                    Error = "CL002300"
                };
            }

            var collectorsGems = await _collectorGemRepo.GetForSingleCollectorAsync(collectorId);

            if (collectorsGems is null)
            {
                _logger.LogError("Repo error during fetch of Collector Gems. Leaving function.");
                return new GenericResponse<List<StoresPurchasedFromOutputModel>>
                {
                    Error = "CL002301"
                };
            }

            if (collectorsGems.Count == 0)
            {
                _logger.LogDebug("No gems found for this collector. Returning empty list.");
                return new GenericResponse<List<StoresPurchasedFromOutputModel>>
                {
                    Data = new List<StoresPurchasedFromOutputModel>()
                };
            }

            var gemStores = collectorsGems.Select(g => g.CreatorId).Distinct().ToList();

            var storesPurchasedFromOutputModel = new List<StoresPurchasedFromOutputModel>();

            foreach (var store in gemStores)
            {
                var storeInfo = await _storeRepo.GetByCreatorId(store);

                if (storeInfo is null)
                {
                    _logger.LogWarning("Error retrieving Store info for creator. Leaving Function.");
                    _logger.LogInformation($"CreatorId: {store}");
                    return new GenericResponse<List<StoresPurchasedFromOutputModel>>
                    {
                        Error = "CL002302"
                    };
                }

                var collectionsPurchFrom = collectorsGems.Where(g => g.CreatorId == store).Select(g => g.CollectionId).Distinct().ToList();
                var gemsCountForStore = collectorsGems.Count(g => g.CreatorId == store);

                storesPurchasedFromOutputModel.Add(new StoresPurchasedFromOutputModel
                {
                    StoreName = storeInfo.Name,
                    StoreTag = storeInfo.UrlStoreTag,
                    StoreLogoImageId = storeInfo.LogoImageId,
                    CollectionCount = collectionsPurchFrom.Count,
                    GemCount = gemsCountForStore
                });
            }

            return new GenericResponse<List<StoresPurchasedFromOutputModel>>
            {
                Data = storesPurchasedFromOutputModel
            };
        }

        public async Task<GenericResponse<PurchAllCollectionsOutputModel>> FetchCollectionsOneStoreContainingPurchases(
                                                string collectorId, PurchCollectionsInputModel purchCollectionsInputModel)
        {
            _logger.LogDebug("Entered FetchCollectionsOneStoreContainingPurchases function");

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Missing or Empty collectorId. Leaving function.");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Error = "CL002400"
                };
            }

            if (string.IsNullOrEmpty(purchCollectionsInputModel.StoreTag))
            {
                _logger.LogError("Missing or Empty Store Tag parameter. Leaving function.");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Error = "CL002401"
                };
            }

            if (purchCollectionsInputModel.NumberOfGems < 1)
            {
                _logger.LogError("Invalid Number of Gems parameter. Must be >= 1. Leaving function.");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Error = "CL002410"
                };
            }

            var storeTagInfo = await _storeTagRepo.GetAsync(purchCollectionsInputModel.StoreTag.ToLower());

            if (storeTagInfo is null)
            {
                _logger.LogError("Cannot retrieve store tag details. Leaving function.");
                _logger.LogInformation($"StoreTag parameter: {purchCollectionsInputModel.StoreTag} | CollectorId: {collectorId}");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Error = "CL002402"
                };
            }

            if (string.IsNullOrEmpty(storeTagInfo.CreatorId))
            {
                _logger.LogError("StoreTag record had null or empty CreatorId. Leaving function.");
                _logger.LogInformation($"StoreTag parameter: {purchCollectionsInputModel.StoreTag} | CollectorId: {collectorId}");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Error = "CL002403"
                };
            }

            var storeData = await _storeRepo.GetByCreatorId(storeTagInfo.CreatorId);
            if (storeData is null)
            {
                _logger.LogError("Cannot retrieve store details. Leaving function.");
                _logger.LogInformation($"CreatorId: {storeTagInfo.CreatorId}");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Error = "CL002409"
                };
            }


            var collectorGemsForStore = await _collectorGemRepo.GetForSingleCreatorAndCollector(collectorId, storeTagInfo.CreatorId);

            if (collectorGemsForStore is null)
            {
                _logger.LogError("Repo error during fetch of collector gems by collector and creator ids. Leaving function.");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Error = "CL002404"
                };
            }

            if (collectorGemsForStore.Count == 0)
            {
                _logger.LogDebug("No collectors gems found for this creator and collector. Returning empty list.");
                return new GenericResponse<PurchAllCollectionsOutputModel>
                {
                    Data = new PurchAllCollectionsOutputModel()
                    {
                        StoreName = storeData.Name,
                        StoreLogoImageId = storeData.LogoImageId,
                        Collections = new List<PurchCollectionsOutputModel>()
                    }
                };
            }

            var collectionsList = collectorGemsForStore.Select(g => g.CollectionId).Distinct().ToList();

            var purchCollectionsOutputList = new List<PurchCollectionsOutputModel>();

            foreach (var collectionId in collectionsList)
            {
                var collectorGemsInCollection = await _collectorGemRepo.GetForSingleCollectorAndCollection(collectorId, collectionId);
                if (collectorGemsInCollection == null)
                {
                    _logger.LogError("Repo error fetching CollectorGems. Leaving Function.");
                    return new GenericResponse<PurchAllCollectionsOutputModel>
                    {
                        Error = "CL002411"
                    };
                }

                var collectorGemsData = 
                    await GetAllGemsFromOneCollection(collectionId,
                    collectorGemsInCollection, "CL0024");

                if (!string.IsNullOrEmpty(collectorGemsData.Error))
                {
                    return new GenericResponse<PurchAllCollectionsOutputModel>()
                    {
                        Error = collectorGemsData.Error
                    };
                }

                var purchCollection = new PurchCollectionsOutputModel
                {
                    CollectionId = collectorGemsData.Data.CollectionId,
                    CollectionName = collectorGemsData.Data.CollectionName,
                    GemCount = collectorGemsData.Data.GemsForCreatorToOpen + 
                               collectorGemsData.Data.PurchasedGems.Aggregate(0, (current, gem) => current + gem.GemCount),
                    CreatorToOpenCount = collectorGemsData.Data.GemsForCreatorToOpen,
                    GemSelection = new List<PurchCollectionGemsOutputModel>()
                };

                foreach (var gem in collectorGemsData.Data.PurchasedGems)
                {
                    purchCollection.GemSelection.Add(new PurchCollectionGemsOutputModel
                    {
                        GemId = gem.GemId,
                        GemName = gem.GemName,
                        ImageId = gem.ImageId,
                        SizePercentage = gem.SizePercentage,
                        PositionXPercentage = gem.PositionXPercentage,
                        PositionYPercentage = gem.PositionYPercentage,
                        Rarity = gem.Rarity,
                        Quantity = gem.GemCount
                    });
                }

                purchCollectionsOutputList.Add(purchCollection);
            }

            return new GenericResponse<PurchAllCollectionsOutputModel>
            {
                Data = new PurchAllCollectionsOutputModel()
                {
                    StoreName = storeData.Name,
                    StoreLogoImageId = storeData.LogoImageId,
                    Collections = purchCollectionsOutputList
                }

            };
        }


        public async Task<GenericResponse<PurchAllGemsCollectionOutputModel>> FetchAllPurchasedGemsInOneCollection(
                                        string collectorId, PurchAllGemsCollectionInputModel purchAllGemsCollectionInputModel)
        {
            _logger.LogDebug("Entered FetchAllPurchasedGemsInOneCollection function");

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Missing or empty Collector Id Parameter. Leaving Function.");
                return new GenericResponse<PurchAllGemsCollectionOutputModel>
                {
                    Error = "CL002500"
                };
            }

            if (string.IsNullOrEmpty(purchAllGemsCollectionInputModel.CollectionId))
            {
                _logger.LogError("Missing or empty Collection Id Parameter. Leaving Function.");
                return new GenericResponse<PurchAllGemsCollectionOutputModel>
                {
                    Error = "CL002501"
                };
            }

            var collectorGemsInCollection = await _collectorGemRepo.GetForSingleCollectorAndCollection(collectorId, purchAllGemsCollectionInputModel.CollectionId);
            if (collectorGemsInCollection == null)
            {
                _logger.LogError("Repo error fetching CollectorGems. Leaving Function.");
                return new GenericResponse<PurchAllGemsCollectionOutputModel>
                {
                    Error = "CL002502"
                };
            }

            if (collectorGemsInCollection.Count == 0)
            {
                _logger.LogDebug("No CollectorGems found so returning empty list");
                return new GenericResponse<PurchAllGemsCollectionOutputModel>
                {
                    Data = new PurchAllGemsCollectionOutputModel
                    {
                        GemsForCreatorToOpen = 0,
                        PurchasedGems = new List<PurchGemOutputModel>()
                    }
                };
            }

            return await GetAllGemsFromOneCollection(purchAllGemsCollectionInputModel.CollectionId,
                    collectorGemsInCollection,"CL0025");
        }


        private async Task<GenericResponse<PurchAllGemsCollectionOutputModel>> GetAllGemsFromOneCollection(string collectionId, 
            List<CollectorGem> collectorGemsList,
            string errorPrefix)
        {
            _logger.LogDebug("Entered GetAllGemsFromOneCollection function.");

            var collectionData = await _collectionRepo.GetSingleCollectionAnyStatus(collectionId);
            if (collectionData == null)
            {
                _logger.LogError("Repo error fetching Collection Data. Leaving Function.");
                return new GenericResponse<PurchAllGemsCollectionOutputModel>
                {
                    Error = errorPrefix + "90"
                };
            }

            var storeData = await _storeRepo.GetByCreatorId(collectionData.CreatorId);
            if (storeData == null)
            {
                _logger.LogError("Repo error fetching Store Data. Leaving Function.");
                return new GenericResponse<PurchAllGemsCollectionOutputModel>
                {
                    Error = errorPrefix + "91"
                };
            }

            var gemsForCreatorToOpen = collectorGemsList.Count(g => g.Visible == false);
            var purchAllGemsCollectionModel = new PurchAllGemsCollectionOutputModel
            {
                CollectionId = collectionId,
                CollectionName = collectionData.Name,
                StoreName = storeData.Name,
                StoreTag = storeData.UrlStoreTag,
                StoreLogoImageId = storeData.LogoImageId,
                GemsForCreatorToOpen = gemsForCreatorToOpen,
                PurchasedGems = new List<PurchGemOutputModel>()
            };

            var allVisibleGems = collectorGemsList.Where(g => g.Visible == true).ToList();

            foreach (var gem in allVisibleGems)
            {
                if (purchAllGemsCollectionModel.PurchasedGems.Exists(g => g.GemId == gem.GemId))
                {
                    var gemInList = purchAllGemsCollectionModel.PurchasedGems.FirstOrDefault(g => g.GemId == gem.GemId);
                    gemInList.GemCount += 1;
                }
                else
                {
                    var gemInfo = await _gemRepo.FetchSingleGemAnyStatus(gem.GemId);
                    if (gemInfo is null)
                    {
                        _logger.LogError("Unable to find Gem Info for Collector Gem. Leaving Function.");
                        return new GenericResponse<PurchAllGemsCollectionOutputModel>
                        {
                            Error = errorPrefix + "92"
                        };
                    }

                    if (gemInfo.CollectionId != collectionId)
                    {
                        _logger.LogError("Found Gem but for different Collection. Leaving Function.");
                        return new GenericResponse<PurchAllGemsCollectionOutputModel>
                        {
                            Error = errorPrefix + "93"
                        };
                    }

                    purchAllGemsCollectionModel.PurchasedGems.Add(new PurchGemOutputModel
                    {
                        GemId = gem.GemId,
                        GemName = gemInfo.Name,
                        ImageId = gemInfo.ImageId,
                        PositionXPercentage = gemInfo.PositionXPercentage,
                        PositionYPercentage = gemInfo.PositionYPercentage,
                        Rarity = gemInfo.Rarity,
                        SizePercentage = gemInfo.SizePercentage,
                        GemCount = 1
                    });
                }
            }

            return new GenericResponse<PurchAllGemsCollectionOutputModel>()
            {
                Data = purchAllGemsCollectionModel
            };
        }
    }

}
