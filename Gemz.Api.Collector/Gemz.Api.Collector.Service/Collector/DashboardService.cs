using Gemz.Api.Collector.Data.Repository;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Collector.Service.Collector
{
    public class DashboardService : IDashboardService
    {
        private readonly ICollectorGemRepository _collectorGemRepo;
        private readonly ICollectorPackRepository _collectorPackRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ICollectorGemRepository collectorGemRepo, 
                                ICollectorPackRepository collectorPackRepo,
                                IStoreRepository storeRepo,
                                ILogger<DashboardService> logger)
        {
            _collectorGemRepo = collectorGemRepo;
            _collectorPackRepo = collectorPackRepo;
            _storeRepo = storeRepo;
            _logger = logger;
        }

        public async Task<GenericResponse<TotalGemsOutputModel>> GetTotalGemsPurchased(string collectorId)
        {
            _logger.LogDebug("Entered GetTotalGemsPurchased function");

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Missing or Empty Collector Id Parameter. Leaving Function.");
                return new GenericResponse<TotalGemsOutputModel> 
                { 
                    Error = "CL002700"
                };
            }

            var allCollectorsGems = await _collectorGemRepo.GetForSingleCollectorAsync(collectorId);

            if (allCollectorsGems is null)
            {
                _logger.LogError("Repo Error during fetch of Collector Gems. Leaving function.");
                return new GenericResponse<TotalGemsOutputModel>
                {
                    Error = "CL002701"
                };
            }

            // And need to get unopened packs x 5
            var collectorPacks = await _collectorPackRepo.FetchUnopenedPacksForCollector(collectorId);
            if (collectorPacks is null)
            {
                _logger.LogError("Repo Error during fetch of Unopened Collector Packs. Leaving function.");
                return new GenericResponse<TotalGemsOutputModel>
                {
                    Error = "CL002702"
                };
            }

            var unopenedGems = collectorPacks.Count * 5;
            var purchasedPacks = (allCollectorsGems.Count / 5) + collectorPacks.Count;

            return new GenericResponse<TotalGemsOutputModel>
            {
                Data = new TotalGemsOutputModel
                {
                    TotalGemsPurchased = allCollectorsGems.Count + unopenedGems,
                    TotalPacksPurchased = purchasedPacks
                }
            };
        }

        public async Task<GenericResponse<TotalGemsOpenedOutputModel>> GetTotalGemsOpened(string collectorId)
        {
            _logger.LogDebug("Entered GetTotalGemsOpened function");

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Missing or Empty Collector Id Parameter. Leaving Function.");
                return new GenericResponse<TotalGemsOpenedOutputModel>
                {
                    Error = "CL002800"
                };
            }

            var allCollectorsGems = await _collectorGemRepo.GetForSingleCollectorAsync(collectorId);

            if (allCollectorsGems is null)
            {
                _logger.LogError("Repo Error during fetch of Collector Gems. Leaving function.");
                return new GenericResponse<TotalGemsOpenedOutputModel>
                {
                    Error = "CL002801"
                };
            }

            var revealedGemsCount = allCollectorsGems.Where(g => g.Visible == true).Count();
            var creatorToOpenCount = allCollectorsGems.Where(g => g.Visible == false).Count();

            return new GenericResponse<TotalGemsOpenedOutputModel>
            {
                Data = new TotalGemsOpenedOutputModel
                {
                    TotalGemsRevealed = revealedGemsCount,
                    TotalGemsForCreatorOpening = creatorToOpenCount,
                    TotalOpenedGems = revealedGemsCount + creatorToOpenCount
                }
            };
        }


        public async Task<GenericResponse<TotalGemsUnopenedOutputModel>> GetTotalGemsUnopened(string collectorId)
        {
            _logger.LogDebug("Entered GetTotalGemsUnopened function");

            if (string.IsNullOrEmpty(collectorId))
            {
                _logger.LogError("Missing or Empty Collector Id Parameter. Leaving Function.");
                return new GenericResponse<TotalGemsUnopenedOutputModel>
                {
                    Error = "CL002900"
                };
            }

            var allUnopenedCollectorsPacks = await _collectorPackRepo.FetchUnopenedPacksForCollector(collectorId);

            if (allUnopenedCollectorsPacks is null)
            {
                _logger.LogError("Repo Error during fetch of Collector Packs. Leaving function.");
                return new GenericResponse<TotalGemsUnopenedOutputModel>
                {
                    Error = "CL002901"
                };
            }

            return new GenericResponse<TotalGemsUnopenedOutputModel>
            {
                Data = new TotalGemsUnopenedOutputModel
                {
                    TotalUnopenedPacks = allUnopenedCollectorsPacks.Count,
                    TotalUnopenedGems = allUnopenedCollectorsPacks.Count * 5
                }
            };
        }

        public async Task<GenericResponse<TotalGemsByCreatorOutputModel>> GetTotalGemsByCreator(string collectorId)
        {
            _logger.LogDebug("Entered GetTotalGemsByCreator function");

            if (string.IsNullOrWhiteSpace(collectorId))
            {
                _logger.LogError("Missing or Empty Collector Id parameter.  Leaving function.");
                return new GenericResponse<TotalGemsByCreatorOutputModel>
                {
                    Error = "CL003000"
                };
            }

            var allCollectorGems = await _collectorGemRepo.GetForSingleCollectorAsync(collectorId);
            if (allCollectorGems is null)
            {
                _logger.LogError("Repo Error during fetch of Collector Gems. Leaving function.");
                return new GenericResponse<TotalGemsByCreatorOutputModel>
                {
                    Error = "CL003001"
                };
            }

            var allCollectorUnopenedPacks = await _collectorPackRepo.FetchUnopenedPacksForCollector(collectorId);
            if (allCollectorUnopenedPacks is null)
            {
                _logger.LogError("Repo Error during fetch of Collector Packs. Leaving function.");
                return new GenericResponse<TotalGemsByCreatorOutputModel>
                {
                    Error = "CL003002"
                };
            }

            var creatorListGems = allCollectorGems.Select(g => g.CreatorId).Distinct().ToList();
            var creatorListPacks = allCollectorUnopenedPacks.Select(p => p.CreatorId).Distinct().ToList();
            var statsByCreator = new TotalGemsByCreatorOutputModel()
            {
                TotalGemsByCreator = new List<TotGemsPurchByCreatorOutputModel>()
            };

            var creatorIdAndTag = new Dictionary<string, string>();


            _logger.LogDebug("Building stats from collector_gems data");
            foreach (var creatorId in creatorListGems)
            {
                _logger.LogInformation($"Building stats for CreatorId: {creatorId}");

                var collectorOpenedGemsForCreator = allCollectorGems
                    .Where(g => g.CreatorId == creatorId && g.Visible == true).ToList();
                var collectorWaitingGemsForCreator = allCollectorGems
                    .Where(g => g.CreatorId == creatorId && g.Visible == false).ToList();

                var creatorStoreData = await _storeRepo.GetByCreatorId(creatorId);
                if (creatorStoreData is null)
                {
                    _logger.LogError("Repo error during fetch of Creator Store Data. Leaving funnction.");
                    _logger.LogInformation($"CreatorId: {creatorId}");
                    return new GenericResponse<TotalGemsByCreatorOutputModel>
                    {
                        Error = "CL003003"
                    };
                }

                creatorIdAndTag.Add(creatorId, creatorStoreData.UrlStoreTag);

                var statsData = new TotGemsPurchByCreatorOutputModel()
                {
                    CreatorStoreName = creatorStoreData.Name,
                    CreatorStoreTag = creatorStoreData.UrlStoreTag,
                    CreatorLogoImageId = creatorStoreData.LogoImageId,
                    TotalGemsOpened = collectorOpenedGemsForCreator.Count,
                    TotalGemsWaitingForCreatorReveal = collectorWaitingGemsForCreator.Count,
                    TotalGemsPurchased = collectorOpenedGemsForCreator.Count + collectorWaitingGemsForCreator.Count,
                    TotalPacksUnopened = 0,
                    TotalGemsUnopened = 0
                };

                _logger.LogDebug("Added open Gem stats for Creator");

                statsByCreator.TotalGemsByCreator.Add(statsData);
            }

            // Now go through unopened packs and add data

            _logger.LogDebug("Adding to stats from Collector_Packs data");

            foreach (var creatorId in creatorListPacks)
            {
                _logger.LogInformation($"Adding pack stats for CreatorId: {creatorId}");

                var creatorAlreadyHasStats = creatorIdAndTag.ContainsKey(creatorId);

                if (creatorAlreadyHasStats)
                {
                    // Adding to existing entry in model
                    var creatorStoreTag = creatorIdAndTag[creatorId];

                    var statsEntry = statsByCreator.TotalGemsByCreator.FirstOrDefault(s => s.CreatorStoreTag == creatorStoreTag);
                    if (statsEntry is null) 
                    {
                        _logger.LogError("Unable to select stats data from model built during gems stats buildup. Leaving function.");
                        return new GenericResponse<TotalGemsByCreatorOutputModel>
                        {
                            Error = "CL003004"
                        };
                    }

                    var collectorPacksForCreator = allCollectorUnopenedPacks.Where(p => p.CreatorId == creatorId).ToList();

                    statsEntry.TotalGemsPurchased += (collectorPacksForCreator.Count * 5);
                    statsEntry.TotalPacksUnopened = collectorPacksForCreator.Count;
                    statsEntry.TotalGemsUnopened = collectorPacksForCreator.Count * 5;

                    _logger.LogDebug("Updated stats entry for creator");
                }
                else
                {
                    // Creating new entry in model

                    var creatorStoreData = await _storeRepo.GetByCreatorId(creatorId);
                    if (creatorStoreData is null)
                    {
                        _logger.LogError("Repo error during fetch of Creator Store Data. Leaving funnction.");
                        _logger.LogInformation($"CreatorId: {creatorId}");
                        return new GenericResponse<TotalGemsByCreatorOutputModel>
                        {
                            Error = "CL003005"
                        };
                    }

                    var collectorPacksForCreator = allCollectorUnopenedPacks.Where(p => p.CreatorId == creatorId).ToList();

                    var statsData = new TotGemsPurchByCreatorOutputModel()
                    {
                        CreatorStoreName = creatorStoreData.Name,
                        CreatorStoreTag = creatorStoreData.UrlStoreTag,
                        CreatorLogoImageId = creatorStoreData.LogoImageId,
                        TotalGemsOpened = 0,
                        TotalGemsWaitingForCreatorReveal = 0,
                        TotalGemsPurchased = collectorPacksForCreator.Count * 5,
                        TotalPacksUnopened = collectorPacksForCreator.Count,
                        TotalGemsUnopened = collectorPacksForCreator.Count * 5
                    };

                    _logger.LogDebug("Added open Gem stats for Creator");

                    statsByCreator.TotalGemsByCreator.Add(statsData);
                }
            }

            var orderedByMostPurch = statsByCreator.TotalGemsByCreator.OrderByDescending(g => g.TotalGemsPurchased).ToList();

            statsByCreator.TotalGemsByCreator = orderedByMostPurch;


            return new GenericResponse<TotalGemsByCreatorOutputModel>
            {
                Data = statsByCreator
            };
        }
    }
}
