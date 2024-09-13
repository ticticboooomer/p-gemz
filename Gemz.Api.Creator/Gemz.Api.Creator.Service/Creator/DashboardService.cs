using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Creator.Service.Creator
{
    public class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly ICollectionRepository _collectionRepo;
        private readonly ICreatorToOpenRepository _creatorToOpenRepo;

        public DashboardService(ICollectionRepository collectionRepo, 
                                ICreatorToOpenRepository creatorToOpenRepo,
                                ILogger<DashboardService> logger)
        {
            _logger = logger;
            _collectionRepo = collectionRepo;
            _creatorToOpenRepo = creatorToOpenRepo;
        }

        public async Task<GenericResponse<CollectionStatsOutputModel>> GetCollectionStats(string creatorId)
        {
            _logger.LogDebug("Entered getCollectionStats in Dashboard Service.");

            if (string.IsNullOrEmpty(creatorId))
            {
                _logger.LogError("Missing or invalid creatorId passed in. Exiting function.");
                return new GenericResponse<CollectionStatsOutputModel>()
                {
                    Error = "CR001000"
                };
            }

            var collectionList = await _collectionRepo.GetAllCollectionsByCreatorId(creatorId);

            if (collectionList == null)
            {
                _logger.LogError("Repo error fetching collections list. Exiting function.");
                return new GenericResponse<CollectionStatsOutputModel>()
                {
                    Error = "CR001001"
                };
            }

            var collectionStats = new CollectionStatsOutputModel()
            {
                TotalCollections = collectionList.Count,
                TotalPublishedCollections = collectionList.Where(c => c.PublishedStatus == 1).Count(),
                TotalUnpublishedCollections = collectionList.Where(c => c.PublishedStatus == 0).Count()
            };

            _logger.LogDebug("Returning with stats");
            return new GenericResponse<CollectionStatsOutputModel>()
            {
                Data = collectionStats,
            };
        }

        public async Task<GenericResponse<GemsToBeOpenedStatsOutputModel>> GetGemsToBeOpenedStats(string creatorId)
        {
            _logger.LogDebug("Entered GetGemsToBeOpenedStats in Dashboard Service");

            if (string.IsNullOrEmpty(creatorId))
            {
                _logger.LogError("Missing or invalid creatorId passed in. Exiting function.");
                return new GenericResponse<GemsToBeOpenedStatsOutputModel>()
                {
                    Error = "CR001100"
                };
            }

            var gemsToBeOpened = await _creatorToOpenRepo.GetAllForOneCreator(creatorId);

            if (gemsToBeOpened == null)
            {
                _logger.LogError("Repo error fetching CreatorToOpen gems list. Exiting function.");
                return new GenericResponse<GemsToBeOpenedStatsOutputModel>()
                {
                    Error = "CR001101"
                };
            }

            _logger.LogDebug("Returning with Gems to be opened stats");

            return new GenericResponse<GemsToBeOpenedStatsOutputModel>
            {
                Data = new GemsToBeOpenedStatsOutputModel
                {
                    NumberOfGemsToBeOpened = gemsToBeOpened.Count
                }
            };
        }
    }
}
