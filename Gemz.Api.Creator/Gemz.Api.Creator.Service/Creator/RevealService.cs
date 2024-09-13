using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Creator.Service.Creator
{
    public class RevealService : IRevealService
    {
        private readonly ICreatorToOpenRepository _creatorToOpenRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ICollectorGemRepository _collectorGemRepo;
        private readonly ICollectionRepository _collectionRepo;
        private readonly IGemRepository _gemRepo;
        private readonly ILogger<RevealService> _logger;

        public RevealService(ICreatorToOpenRepository creatorToOpenRepo,
                             IAccountRepository accountRepo,
                             ICollectorGemRepository collectorGemRepo,
                             ICollectionRepository collectionRepo,
                             IGemRepository gemRepo,
                             ILogger<RevealService> logger)
        {
            _creatorToOpenRepo = creatorToOpenRepo;
            _accountRepo = accountRepo;
            _collectorGemRepo = collectorGemRepo;
            _collectionRepo = collectionRepo;
            _gemRepo = gemRepo;
            _logger = logger;
        }

        public async Task<GenericResponse<GemsToBeRevealedOutputModel>> GetGemsTobeRevealedByCreator(string creatorId)
        {
            _logger.LogDebug("Entered GetGemsTobeRevealedByCreator in RevealService.");

            if (string.IsNullOrEmpty(creatorId))
            {
                _logger.LogError("Missing or invalid creatorId parameter. Leaving function.");
                return new GenericResponse<GemsToBeRevealedOutputModel>
                {
                    Error = "CR001200"
                };
            }

            var allCreatorToOpenGems = await _creatorToOpenRepo.GetAllForOneCreator(creatorId);

            if (allCreatorToOpenGems == null)
            {
                _logger.LogError("Repo error on fetch of creator_to_open records. Leaving function.");
                return new GenericResponse<GemsToBeRevealedOutputModel>
                {
                    Error = "CR001201"
                };
            }

            var sortedCreatorToOpenGems = allCreatorToOpenGems.OrderBy(g => g.CreatedOn).ToList();

            var gemsToBeRevealedOutputList = new GemsToBeRevealedOutputModel
            {
                GemsToBeRevealed = new List<GemToBeRevealed>()
            };

            foreach (var creatorToOpenEntry in sortedCreatorToOpenGems)
            {
                var gemToBeRevealed = new GemToBeRevealed();

                _logger.LogDebug("Finding account that owns the Gem to be revealed.");
                _logger.LogInformation($"creator_to_open Id: {creatorToOpenEntry.Id}");
                var collector = await _accountRepo.GetAsync(creatorToOpenEntry.CollectorId);

                if (collector == null)
                {
                    _logger.LogWarning("Unable to find Collector details for CreatorToOpen document. Skipping Document.");
                    _logger.LogInformation($"CreatorToOpen Id: {creatorToOpenEntry.Id}  |  CollectorId: {creatorToOpenEntry.CollectorId}");
                    continue;
                }

                var collectorGem = await _collectorGemRepo.GetSingleCollectorGem(creatorToOpenEntry.CollectorGemsId);
                if (collectorGem == null)
                {
                    _logger.LogWarning("Unable to find CollectorGems details for this CreatorToOpen document. Skipping Document.");
                    _logger.LogInformation($"CreatorToOpen Id: {creatorToOpenEntry.Id}  |  CollectorGems Id: {creatorToOpenEntry.CollectorGemsId}");
                    continue;
                }

                if (collectorGem.CreatorId != creatorId)
                {
                    _logger.LogWarning("Found CollectorGems details but not for this Creator Skipping Document.");
                    _logger.LogInformation($"CreatorToOpen Id: {creatorToOpenEntry.Id}  |  CollectorGems Id: {creatorToOpenEntry.CollectorGemsId}");
                    continue;
                }

                var collectionDetails = await _collectionRepo.GetAsyncAnyStatus(collectorGem.CollectionId);
                if (collectionDetails == null)
                {
                    _logger.LogWarning("Unable to find collection associated with this CollectorGems entry. Skipping document.");
                    _logger.LogInformation($"CollectorGems Id: {collectorGem.Id}  |  collectionId: {collectorGem.CollectionId}");
                    continue;
                }

                if (collectionDetails.CreatorId != creatorId)
                {
                    _logger.LogWarning("Found collection associated with this CollectorGems entry, but not for this Creator. Skipping document.");
                    _logger.LogInformation($"CollectorGems Id: {collectorGem.Id}  |  collectionId: {collectorGem.CollectionId}");
                    continue;
                }

                _logger.LogDebug("Adding gem to be reavealed to return list.");
                gemToBeRevealed.CreatorToOpenId = creatorToOpenEntry.Id;
                gemToBeRevealed.CollectorGemsId = creatorToOpenEntry.CollectorGemsId;
                gemToBeRevealed.CollectionName = collectionDetails.Name;
                gemToBeRevealed.CollectorName = collector.TwitchUsername;

                var formattedDateAndTime = FormatDateAndTime(creatorToOpenEntry.CreatedOn);
                gemToBeRevealed.WaitingSinceDate = formattedDateAndTime.GetValueOrDefault("dateOnly");
                gemToBeRevealed.WaitingSinceTime = formattedDateAndTime.GetValueOrDefault("timeOnly");

                gemsToBeRevealedOutputList.GemsToBeRevealed.Add(gemToBeRevealed);

            }

            _logger.LogDebug("Built list of Gems for creator to reveal. Returning data.");
            return new GenericResponse<GemsToBeRevealedOutputModel>
            {
                Data = gemsToBeRevealedOutputList
            };
        }

        public async Task<GenericResponse<SingleGemRevealOutputModel>> RevealSingleGem(string creatorId, SingleGemRevealInputModel singleGemRevealInputModel)
        {
            _logger.LogDebug("Entered ReavealSingleGem function in RevealService");

            if (string.IsNullOrEmpty(creatorId))
            {
                _logger.LogError("Missing or Invalid creatorId parameter. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001300"
                };
            }

            if (string.IsNullOrEmpty(singleGemRevealInputModel.CreatorToOpenId))
            {
                _logger.LogError("Missing or Invalid CreatorToOpen Id. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001310"
                };
            }

            var creatorToOpenEntry = await _creatorToOpenRepo.GetById(singleGemRevealInputModel.CreatorToOpenId);
            if (creatorToOpenEntry == null)
            {
                _logger.LogError("Repo error while fetching CreatorToOpen document. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001311"
                };
            }

            if (creatorToOpenEntry.CreatorId != creatorId)
            {
                _logger.LogError("Found CreatorToOpen document, but not for this Creator. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001312"
                };
            }

            var collectorGem = await _collectorGemRepo.GetSingleCollectorGem(creatorToOpenEntry.CollectorGemsId);

            if (collectorGem == null) 
            {
                _logger.LogError("Repo error during fetch of Collector Gem. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001302"
                };
            }

            if (collectorGem.CreatorId != creatorId)
            {
                _logger.LogError("Collector Gem found but is not associated with this Creator. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001303"
                };
            }

            if (collectorGem.Visible == true)
            {
                _logger.LogError("This Collector Gem has already been revealed. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001313"
                };

            }

            if (collectorGem.CollectorId != creatorToOpenEntry.CollectorId)
            {
                _logger.LogError("CollectorGem found but CollectorId not matching CreatorToOpen Entry. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001301"
                };
            }

            var collectorDetails = await _accountRepo.GetAsync(collectorGem.CollectorId);
            if (collectorDetails == null)
            {
                _logger.LogError("Repo error during fetch of Collector Details. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001304"
                };
            }

            var collectionDetails = await _collectionRepo.GetAsyncAnyStatus(collectorGem.CollectionId);
            if (collectionDetails == null)
            {
                _logger.LogError("Repo error during fetch of Collection Details for this Gem. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001305"
                };
            }

            if (collectionDetails.CreatorId != creatorId)
            {
                _logger.LogError("Collection found but not for this Creator. Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001308"
                };
            }

            var gemDetails = await _gemRepo.GetAsync(collectorGem.GemId);
            if (gemDetails == null)
            {
                _logger.LogError($"Repo error during fetch of Gem details for GemId: {collectorGem.GemId}");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001306"
                };
            }

            if (gemDetails.CreatorId != creatorId)
            {
                _logger.LogError("Gem found but not for this Creator.Leaving function.");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001309"
                };
            }


            collectorGem.Visible = true;
            var collectorGemUpdated = await _collectorGemRepo.UpdateSingle(collectorGem);
            if (collectorGemUpdated == null) 
            {
                _logger.LogError($"Repo error during update of CollectorGem to visible. Leaving function.");
                _logger.LogInformation($"CollectorGem.Id: {collectorGem.Id}");
                return new GenericResponse<SingleGemRevealOutputModel>
                {
                    Error = "CR001307"
                };
            }

            var isDeleted = await _creatorToOpenRepo.DeleteSingle(creatorToOpenEntry.Id);
            if (!isDeleted)
            {
                // Decided to NOT return an error when this repo error occurs. Just logging it.
                // Reason: The Collector Gem has been updated by this point so may as well return the details
                //         If this error occurs then the item will fail any subsequent attempts due to
                //         Already being set to visible
                _logger.LogWarning("Delete of CreatorToOpen document failed.");
                _logger.LogInformation($"CreatorToOpen Id: {creatorToOpenEntry.Id}");
            }

            var singleGemRevealOutputModel = new SingleGemRevealOutputModel()
            {
                CollectorName = collectorDetails.TwitchUsername,
                CollectionName = collectionDetails.Name,
                GemId = collectorGem.GemId,
                GemName = gemDetails.Name,
                ImageId = gemDetails.ImageId,
                PositionXPercentage = gemDetails.PositionXPercentage,
                PositionYPercentage = gemDetails.PositionYPercentage,
                PublishedStatus = gemDetails.PublishedStatus,
                Rarity = gemDetails.Rarity,
                SizePercentage = gemDetails.SizePercentage
            };

            _logger.LogDebug("Built Gem information and returning data.");
            return new GenericResponse<SingleGemRevealOutputModel>
            {
                Data = singleGemRevealOutputModel
            };
        }

        private static Dictionary<string, string> FormatDateAndTime(DateTime dateAndTime)
        {
            return new Dictionary<string, string>
        {
            { "dateOnly", dateAndTime.ToString("ddd, dd MMM yyyy") },
            { "timeOnly", dateAndTime.ToString("HH:mm 'UTC'") }
        };
        }

    }
}
