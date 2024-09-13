using Gemz.Api.Collector.Data.Model;
using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Gemz.Api.Collector.Service.Collector
{
    public class CollectorPackService : ICollectorPackService
    {
        private readonly ILogger<CollectorPackService> _logger;
        private readonly ICollectionRepository _collectionRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IGemRepository _gemRepo;
        private readonly ICollectorPackRepository _collectorPackRepo;
        private readonly ICollectorPackOpenedRepository _collectorPackOpenedRepo;
        private readonly ICollectorGemRepository _collectorGemRepo;
        private readonly ICreatorToOpenRepository _creatorToOpenRepo;
        private readonly IOpenPackSessionRepository _openPackSessionRepo;
        private readonly IOptions<GemzDefaultsConfig> _gemzDefaultsConfig;

        public CollectorPackService(ICollectorPackRepository collectorPackRepo,
                                    ICollectionRepository collectionRepo,
                                    IStoreRepository storeRepo,
                                    IGemRepository gemRepo,
                                    ICollectorPackOpenedRepository collectorPackOpenedRepo,
                                    ICollectorGemRepository collectorGemRepo,
                                    ICreatorToOpenRepository creatorToOpenRepo,
                                    IOpenPackSessionRepository openPackSessionRepo,
                                    IOptions<GemzDefaultsConfig> gemzDefaultsConfig,
                                    ILogger<CollectorPackService> logger)
        {
            _collectorPackRepo = collectorPackRepo;
            _collectionRepo = collectionRepo;
            _storeRepo = storeRepo;
            _gemRepo = gemRepo;
            _collectorPackOpenedRepo = collectorPackOpenedRepo;
            _collectorGemRepo = collectorGemRepo;
            _creatorToOpenRepo = creatorToOpenRepo;
            _openPackSessionRepo = openPackSessionRepo;
            _gemzDefaultsConfig = gemzDefaultsConfig;
            _logger = logger;
        }

        public async Task<GenericResponse<List<UnopenedPackOutputModel>>> FetchUnopenedPacksForCollector(string collectorId)
        {
            _logger.LogDebug("Entered FetchUnopenedPacksForCollector function");

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Mising or empty collector id. Leaving Function.");
                return new GenericResponse<List<UnopenedPackOutputModel>>
                {
                    Error = "CL001800"
                };
            }

            var collectorPacks = await _collectorPackRepo.FetchUnopenedPacksForCollector(collectorId);

            if (collectorPacks is null)
            {
                _logger.LogError("There was an error retrieving the Collector Packs. Leaving function.");
                return new GenericResponse<List<UnopenedPackOutputModel>>
                {
                    Error = "CL001801"
                };
            }

            if (collectorPacks.Count == 0)
            {
                _logger.LogInformation("No unopened packs for this collector");
                return new GenericResponse<List<UnopenedPackOutputModel>>
                {
                    Data = new List<UnopenedPackOutputModel>()
                };
            }


            var distinctCollections = collectorPacks.Select(p => p.CollectionId).Distinct().ToList();

            var collectionsData = new List<Collection>();

            foreach (var collection in distinctCollections)
            {
                var collectionInfo = await _collectionRepo.GetSingleCollection(collection);
                if (collectionInfo != null)
                {
                    collectionsData.Add(collectionInfo);
                }
            }

            var distinctCreators = collectionsData.Select(c => c.CreatorId).Distinct().ToList();
            var creatorsData = new List<Store>();
            var unopenedPackOutputModel = new List<UnopenedPackOutputModel>();

            foreach (var creator in distinctCreators)
            {
                var creatorStoreData = await _storeRepo.GetByCreatorId(creator);
                if (creatorStoreData != null)
                {
                    creatorsData.Add(creatorStoreData);
                    unopenedPackOutputModel.Add(new UnopenedPackOutputModel
                    {
                        CreatorStoreName = creatorStoreData.Name,
                        CreatorStoreTag = creatorStoreData.UrlStoreTag,
                        CreatorLogoImageId = creatorStoreData.LogoImageId,
                        UnopenedPackCollections = new List<UnopenedPackCollections>()
                    });
                }
            }

            foreach (var collectorPack in collectorPacks)
            {
                if (collectorPack.CollectorId != collectorId)
                {
                    _logger.LogError("CollectorPacks not for this collector have been retrieved. Leaving Function.");
                    return new GenericResponse<List<UnopenedPackOutputModel>>
                    {
                        Error = "CL001802"
                    };
                }

                var thisCollectionData = collectionsData.FirstOrDefault(c => c.Id == collectorPack.CollectionId);
                if (thisCollectionData == null)
                {
                    _logger.LogError("Collection for this collector pack failed to be retrieved from repo. Leaving Function.");
                    _logger.LogInformation($"CollectionPackId: {collectorPack.Id} | CollectionId: {collectorPack.CollectionId}");
                    return new GenericResponse<List<UnopenedPackOutputModel>>
                    {
                        Error = "CL001805"
                    };
                }
                var thisCreatorData = creatorsData.FirstOrDefault(c => c.CreatorId == thisCollectionData.CreatorId);
                if (thisCreatorData == null)
                {
                    _logger.LogError("Creator for collection of this collector pack failed to be retrieved from repo. Leaving Function.");
                    _logger.LogInformation($"CollectionPackId: {collectorPack.Id} | CollectionId: {collectorPack.CollectionId} | CreatorId: {thisCollectionData.CreatorId}");
                    return new GenericResponse<List<UnopenedPackOutputModel>>
                    {
                        Error = "CL001806"
                    };
                }

                var existingCreatorEntry = unopenedPackOutputModel.FirstOrDefault(c => c.CreatorStoreTag == thisCreatorData.UrlStoreTag);

                if (existingCreatorEntry is null)
                {
                    _logger.LogError("Issues building unopened collector packs for output. Leaving function.");
                    return new GenericResponse<List<UnopenedPackOutputModel>>
                    {
                        Error = "CL001807"
                    };
                }

                var existingCollectionEntry = existingCreatorEntry.UnopenedPackCollections.FirstOrDefault(c => c.CollectionId == collectorPack.CollectionId);
                if (existingCollectionEntry is null)
                {
                    // Adding collection to this creator for first time
                    var newCollectionEntry = new UnopenedPackCollections
                    {
                        CollectionId = collectorPack.CollectionId,
                        CollectionName = thisCollectionData.Name,
                        UnopenedPackCount = 1
                    };
                    existingCreatorEntry.UnopenedPackCollections.Add(newCollectionEntry);
                }
                else
                {
                    // Adding more packs into existing collection
                    existingCollectionEntry.UnopenedPackCount += 1;
                }
            }

            var sortedUnopenedPackOutputModel = unopenedPackOutputModel.OrderBy(p => p.CreatorStoreName).ToList();
            return new GenericResponse<List<UnopenedPackOutputModel>>
            {
                Data = sortedUnopenedPackOutputModel
            };
        }

        public async Task<GenericResponse<OpenPacksOutputModel>> OpenCollectorPacksInCollection(string collectorId, OpenPacksInputModel openPacksInputModel)
        {
            _logger.LogDebug("Entered OpenCollectorPacksInCollection function");

            if (string.IsNullOrEmpty(openPacksInputModel.OpenPacksSessionId))
            {
                _logger.LogError("Missing or empty Session Id. Leaving function.");
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = "CL002015"
                };
            }

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Missing or empty collectorId. Leaving function.");
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = "CL002000"
                };
            }

            if (string.IsNullOrEmpty(openPacksInputModel.CollectionId))
            {
                _logger.LogError("Missing or empty CollectionId. Leaving function.");
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = "CL002001"
                };
            }

            if (openPacksInputModel.NumberOfPacks <= 0)
            {
                _logger.LogError("Invalid Number of packs parameter (<= 0). Leaving function.");
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = "CL002002"
                };
            }

            if (_gemzDefaultsConfig?.Value.PackSize is null || _gemzDefaultsConfig?.Value.PackSize <= 0)
            {
                _logger.LogError("The config setting for PackSize is missing or invalid. Leaving Function.");
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = "CL002008"
                };
            }


            var packsResponse = await FetchPacksToBeOpened(collectorId, openPacksInputModel.CollectionId, openPacksInputModel.NumberOfPacks);
            if (packsResponse.Data is null)
            {
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = packsResponse.Error
                };
            }
            var packsToOpen = packsResponse.Data;

            var gemsResponse = await FetchAllAvailableGemsInCollection(openPacksInputModel.CollectionId);
            if (gemsResponse.Data is null)
            {
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = gemsResponse.Error
                };
            }

            var splitGems = SplitGemsByRarity(gemsResponse.Data);


            var collectionData = await _collectionRepo.GetSingleCollection(openPacksInputModel.CollectionId);
            if (collectionData is null)
            {
                _logger.LogError("Unable to fetch collection details. Leaving Function.");
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = "CL002009"
                };
            }

            var creatorStoreData = await _storeRepo.GetByCreatorId(collectionData.CreatorId);
            if (creatorStoreData is null)
            {
                _logger.LogError("Unable to fetch Creator details. Leaving Function.");
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = "CL002010"
                };
            }

            var openPacksOutputModel = new OpenPacksOutputModel
            {
                CollectionId = collectionData.Id,
                CollectionName = collectionData.Name,
                GemsForCreatorToOpen = 0,
                OpenedGems = new List<OpenPacksGem>()
            };

            var random = new Random();

            foreach (var pack in packsToOpen)
            {
                var gemsCreatedForPack = new List<string>();

                for (var i = 0; i < _gemzDefaultsConfig.Value.PackSize; i++)
                {

                    Gem selectedGem = null;
                    while (selectedGem == null)
                    {
                        var randomNumber = random.NextDouble();
                        if (randomNumber < 0.0001 && splitGems.MythicGems.Count > 0)
                        {
                            // Pick from Mythic
                            selectedGem = AllocateGemFromList(splitGems.MythicGems);
                        }
                        else if (randomNumber < 0.001 && splitGems.LegendaryGems.Count > 0)
                        {
                            // Pick from Legendary
                            selectedGem = AllocateGemFromList(splitGems.LegendaryGems);
                        }
                        else if (randomNumber < 0.01 && splitGems.EpicGems.Count > 0)
                        {
                            // Pick from Epic
                            selectedGem = AllocateGemFromList(splitGems.EpicGems);
                        }
                        else if (randomNumber < 0.1 && splitGems.RareGems.Count > 0)
                        {
                            // Pick from Rare
                            selectedGem = AllocateGemFromList(splitGems.RareGems);
                        }
                        else if (randomNumber < 1 && splitGems.CommonGems.Count > 0)
                        {
                            // Pick from Common
                            selectedGem = AllocateGemFromList(splitGems.CommonGems);
                        }
                    }

                    var collectorGemResponse = await CreateNewCollectorGem(collectionData, selectedGem, collectorId, creatorStoreData.MaxAutoOpenRarity);
                    if (collectorGemResponse.Data is null)
                    {
                        return new GenericResponse<OpenPacksOutputModel>
                        {
                            Error = collectorGemResponse.Error
                        };
                    }
                    var newCollectorGem = collectorGemResponse.Data;

                    gemsCreatedForPack.Add(newCollectorGem.Id);

                    if (!newCollectorGem.Visible)
                    {
                        var creatorToOpenResponse = await CreateNewCreatorToOpenEntry(collectorId, creatorStoreData.CreatorId, newCollectorGem.Id);
                        if (creatorToOpenResponse.Data is null)
                        {
                            return new GenericResponse<OpenPacksOutputModel>
                            {
                                Error = creatorToOpenResponse.Error
                            };
                        }
                        openPacksOutputModel.GemsForCreatorToOpen += 1;
                    }
                    else
                    {
                        var openPacksGem = openPacksOutputModel.OpenedGems.FirstOrDefault(g => g.GemId == selectedGem.Id);
                        if (openPacksGem != null)
                        {
                            openPacksGem.NumberOpened += 1;
                        }
                        else
                        {
                            var newOpenedGem = new OpenPacksGem
                            {
                                GemId = selectedGem.Id,
                                Name = selectedGem.Name,
                                ImageId = selectedGem.ImageId,
                                Rarity = selectedGem.Rarity,
                                PositionXPercentage = selectedGem.PositionXPercentage,
                                PositionYPercentage = selectedGem.PositionYPercentage,
                                SizePercentage = selectedGem.SizePercentage,
                                NumberOpened = 1
                            };
                            openPacksOutputModel.OpenedGems.Add(newOpenedGem);
                        }
                    }

                }

                var collPacksTidyResponse = await ArchiveCollectorPack(pack, gemsCreatedForPack);
                if (collPacksTidyResponse.Error != null)
                {
                    return new GenericResponse<OpenPacksOutputModel>
                    {
                        Error = collPacksTidyResponse.Error
                    };
                }
            }

            var openPackSessionCreateSuccess = await CreateOpenPackSession(openPacksInputModel.OpenPacksSessionId, collectorId, openPacksOutputModel);

            if (!openPackSessionCreateSuccess.Data)
            {
                return new GenericResponse<OpenPacksOutputModel>
                {
                    Error = openPackSessionCreateSuccess.Error
                };
            }

            return new GenericResponse<OpenPacksOutputModel>
            {
                Data = openPacksOutputModel
            };
        }


        private async Task<GenericResponse<bool>> CreateOpenPackSession(string sessionId, string collectorId, OpenPacksOutputModel openPacksOutputModel)
        {
            var newOpenPackSession = new OpenPackSession()
            {
                Id = sessionId,
                CollectionId = openPacksOutputModel.CollectionId,
                CollectionName = openPacksOutputModel.CollectionName,
                CollectorId = collectorId,
                CreatedOn = DateTime.UtcNow,
                GemsForCreatorToOpen = openPacksOutputModel.GemsForCreatorToOpen,
                OpenedGems = new List<OpenPackSessionGem>()
            };

            foreach (var gem in openPacksOutputModel.OpenedGems)
            {
                newOpenPackSession.OpenedGems.Add(new OpenPackSessionGem
                {
                    GemId = gem.GemId,
                    Name = gem.Name,
                    ImageId = gem.ImageId,
                    NumberOpened = gem.NumberOpened,
                    Rarity = gem.Rarity,
                    SizePercentage = gem.SizePercentage,
                    PositionXPercentage = gem.PositionXPercentage,
                    PositionYPercentage = gem.PositionYPercentage
                });
            }

            var createOpenPackSession = await _openPackSessionRepo.CreateOpenPackSessionAsync(newOpenPackSession);

            if (createOpenPackSession is null) 
            {
                _logger.LogError("Repo error during creation of OpenPackSession. Leaving Function.");
                return new GenericResponse<bool>
                {
                    Error = "CL002016"
                };
            }

            return new GenericResponse<bool>
            {
                Data = true
            };
        }


        private async Task<GenericResponse<List<CollectorPack>>> FetchPacksToBeOpened(string collectorId, string collectionId, int numberOfPacks)
        {

            var packsInCollection = await _collectorPackRepo.FetchUnopenedPacksInCollection(collectorId, collectionId);

            if (packsInCollection is null)
            {
                _logger.LogError("Repo Error occured during fetch of unopened collector packs. Leaving Function.");
                return new GenericResponse<List<CollectorPack>>
                {
                    Error = "CL002003"
                };
            }

            if (packsInCollection.Count == 0)
            {
                _logger.LogError("Zero unopened collector packs were retrieved for collection. Leaving Function.");
                _logger.LogInformation($"CollectionId: {collectionId}");
                return new GenericResponse<List<CollectorPack>>
                {
                    Error = "CL002004"
                };
            }

            if (packsInCollection.Count < numberOfPacks)
            {
                _logger.LogError("There were less packs to open then requested for opening. Leaving Function.");
                _logger.LogInformation($"Packs in Repo: {packsInCollection.Count} | NumberOfPacksToOpen: {numberOfPacks}");
                return new GenericResponse<List<CollectorPack>>
                {
                    Error = "CL002005"
                };
            }
            var sortedPacks = packsInCollection.OrderBy(p => p.CreatedOn).ToList();

            var packsToOpen = sortedPacks.Take(numberOfPacks).ToList();

            return new GenericResponse<List<CollectorPack>>
            {
                Data = packsToOpen
            };
        }

        public async Task<GenericResponse<List<Gem>>> FetchAllAvailableGemsInCollection(string collectionId)
        {
            var allGemsInCollection = await _gemRepo.FetchAllGemsForCollection(collectionId);

            if (allGemsInCollection is null)
            {
                _logger.LogError("Repo error fetching Gems in collection. Leaving Function.");
                return new GenericResponse<List<Gem>>
                {
                    Error = "CL002006"
                };
            }

            if (allGemsInCollection.Count == 0)
            {
                _logger.LogError("No Gems found in the collection. Leaving Function.");
                return new GenericResponse<List<Gem>>
                {
                    Error = "CL002007"
                };
            }

            return new GenericResponse<List<Gem>>
            {
                Data = allGemsInCollection
            };
        }

        public async Task<GenericResponse<OpenPackSessionOutputModel>> CheckOpenPackSession(string collectorId, OpenPacksSessionInputModel openPacksSessionInputModel)
        {
            _logger.LogDebug("Entered CheckOpenPackSession function");

            if (string.IsNullOrEmpty(openPacksSessionInputModel.SessionId))
            {
                _logger.LogError("Missing or empty Session Id. Leaving function.");
                return new GenericResponse<OpenPackSessionOutputModel>
                {
                    Error = "CL002200"
                };
            }

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Missing or empty Collector Id. Leaving function.");
                return new GenericResponse<OpenPackSessionOutputModel>
                {
                    Error = "CL002202"
                };
            }

            var existingSession = await _openPackSessionRepo.GetOpenPackSessionAsync(openPacksSessionInputModel.SessionId);

            if (existingSession is null)
            {
                return new GenericResponse<OpenPackSessionOutputModel>
                {
                    Data = new OpenPackSessionOutputModel()
                };
            }

            if (existingSession.CollectorId != collectorId)
            {
                _logger.LogError("Found Open Pack Session but not for this Collector. Leaving Function.");
                return new GenericResponse<OpenPackSessionOutputModel>
                {
                    Error = "CL002201"
                };
            }

            return new GenericResponse<OpenPackSessionOutputModel>
            {
                Data = MapOpenPackSessionToOutputModel(existingSession)
            };
        }


        private static OpenPackSessionOutputModel MapOpenPackSessionToOutputModel(OpenPackSession existingSession)
        {
            var model = new OpenPackSessionOutputModel
            {
                SessionId = existingSession.Id,
                CollectionId = existingSession.CollectionId,
                CollectionName = existingSession.CollectionName,
                GemsForCreatorToOpen = existingSession.GemsForCreatorToOpen,
                OpenedGems = new List<Model.OpenPackSessionGemOutputModel>()
            };

            foreach (var gem in existingSession.OpenedGems)
            {
                model.OpenedGems.Add(new Model.OpenPackSessionGemOutputModel
                {
                    GemId = gem.GemId,
                    ImageId = gem.ImageId,
                    Name = gem.Name,
                    NumberOpened = gem.NumberOpened,
                    PositionXPercentage = gem.PositionXPercentage,
                    PositionYPercentage = gem.PositionYPercentage,
                    Rarity = gem.Rarity,
                    SizePercentage = gem.SizePercentage
                });
            }

            return model;
        }

        private async Task<GenericResponse<CollectorGem>> CreateNewCollectorGem(Collection collectionData, Gem selectedGem, string collectorId, int creatorMaximumOpenRarity)
        {
            var collectorGem = new CollectorGem
            {
                Id = Guid.NewGuid().ToString(),
                CollectorId = collectorId,
                CreatorId = collectionData.CreatorId,
                CollectionId = collectionData.Id,
                GemId = selectedGem.Id,
                Visible = selectedGem.Rarity <= creatorMaximumOpenRarity,
                CreatedOn = DateTime.UtcNow
            };

            var newCollectorGem = await _collectorGemRepo.CreateAsync(collectorGem);
            if (newCollectorGem is null)
            {
                _logger.LogError("Repo Error while creating Collector_Gems record. Leaving Function.");
                return new GenericResponse<CollectorGem>
                {
                    Error = "CL002011"
                };
            }

            return new GenericResponse<CollectorGem>
            {
                Data = newCollectorGem
            };
        }

        private async Task<GenericResponse<CreatorToOpen>> CreateNewCreatorToOpenEntry(string collectorId, string creatorId, string gemId)
        {
            var creatorToOpen = new CreatorToOpen
            {
                Id = Guid.NewGuid().ToString(),
                CollectorId = collectorId,
                CreatorId = creatorId,
                CollectorGemsId = gemId,
                CreatedOn = DateTime.UtcNow
            };
            var newCreatorToOpen = await _creatorToOpenRepo.CreateAsync(creatorToOpen);
            if (newCreatorToOpen is null)
            {
                _logger.LogError("Repo Error while creating CreatorToOpen record. Leaving Function.");
                return new GenericResponse<CreatorToOpen>
                {
                    Error = "CL002012"
                };
            }

            return new GenericResponse<CreatorToOpen>
            {
                Data = newCreatorToOpen
            };
        }

        private async Task<GenericResponse<CollectorPackOpened>> CreateNewCollectorPackOpenedEntry(CollectorPack collectorPack, List<string> gemsCreatedFromPack)
        {
            var collectorPackOpened = new CollectorPackOpened
            {
                Id = Guid.NewGuid().ToString(),
                CollectionId = collectorPack.CollectionId,
                CreatorId = collectorPack.CreatorId,
                CollectorId = collectorPack.CollectorId,
                OriginatingOrderId = collectorPack.OriginatingOrderId,
                OriginatingOrderLineId = collectorPack.OriginatingOrderLineId,
                GemsCreatedFromPack = gemsCreatedFromPack,
                CreatedOn = DateTime.UtcNow
            };

            var newCollectorPackOpened = await _collectorPackOpenedRepo.CreateAsync(collectorPackOpened);
            if (newCollectorPackOpened is null)
            {
                _logger.LogError("Repo error during create of new CollectorPackOpened record. Leaving function.");
                return new GenericResponse<CollectorPackOpened>
                {
                    Error = "CL002013"
                };
            }

            return new GenericResponse<CollectorPackOpened>
            {
                Data = newCollectorPackOpened
            };
        }

        private async Task<GenericResponse<bool>> ArchiveCollectorPack(CollectorPack pack, List<string> gemsCreatedFromPack)
        {
            
            var collPacksOpenedResponse = await CreateNewCollectorPackOpenedEntry(pack, gemsCreatedFromPack);
            if (collPacksOpenedResponse.Data is null)
            {
                return new GenericResponse<bool>
                {
                    Error = collPacksOpenedResponse.Error
                };
            }

            var collPackDelSuccess = await _collectorPackRepo.DeleteAsync(pack.Id);
            if (!collPackDelSuccess)
            {
                _logger.LogError("Repo error during delete of CollectorPack record");
                _logger.LogInformation($"CollectorPackId: {pack.Id}");
                return new GenericResponse<bool>
                {
                    Error = "CL002014"
                };
            }

            return new GenericResponse<bool>
            {
                Data = true
            };
        }

        private static GemsByRarity SplitGemsByRarity(List<Gem> allGems)
        {
            var gemsByRarity = new GemsByRarity
            {
                CommonGems = new List<Gem>(),
                EpicGems = new List<Gem>(),
                LegendaryGems = new List<Gem>(),
                MythicGems = new List<Gem>(),
                RareGems = new List<Gem>()
            };

            foreach (var gem in allGems)
            {
                if (gem.Rarity == 0)
                {
                    gemsByRarity.CommonGems.Add(gem);
                }
                else if (gem.Rarity == 1)
                {
                    gemsByRarity.RareGems.Add(gem);
                }
                else if (gem.Rarity == 2)
                {
                    gemsByRarity.EpicGems.Add(gem);
                }
                else if (gem.Rarity == 3)
                {
                    gemsByRarity.LegendaryGems.Add(gem);
                }
                else if (gem.Rarity == 4)
                {
                    gemsByRarity.MythicGems.Add(gem);
                }
            }

            return gemsByRarity;
        }

        private static Gem AllocateGemFromList(List<Gem> gems)
        {
            var maxGems = gems.Count;

            var selectNumber = RandomNumberGenerator.GetInt32(maxGems);

            return gems[selectNumber];
        }
    }
}
