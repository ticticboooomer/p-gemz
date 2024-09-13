using Gemz.Api.Creator.Service.Creator.Model;
using Gemz.Api.Creator.Data.Model;
using Gemz.Api.Creator.Data.Repository;
using Microsoft.Extensions.Logging;

namespace Gemz.Api.Creator.Service.Creator
{
    public class InterestService : IInterestService
    {
        private readonly IRegisteredInterestRepository _registeredInterestRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ILogger<InterestService> _logger;

        public InterestService(ILogger<InterestService> logger, IRegisteredInterestRepository registeredInterestRepo, IAccountRepository accountRepo)
        {
            _logger = logger;
            _registeredInterestRepo = registeredInterestRepo;
            _accountRepo = accountRepo;
        }

        public async Task<GenericResponse<RegisterInterestOutputModel>> RegisterInterest(string accountId, RegisterInterestInputModel registerInterestInputModel)
        {
            _logger.LogDebug("Entered RegisterInterest function.");

            if (string.IsNullOrEmpty(accountId))
            {
                _logger.LogError("AccountId parameter is null or empty. Leaving function.");
                return new GenericResponse<RegisterInterestOutputModel>()
                {
                    Error = "CR002000"
                };
            }

            var accountDetails = await _accountRepo.GetAsync(accountId);
            if (accountDetails is null)
            {
                _logger.LogError("Repo error fetching account details. Leaving function.");
                return new GenericResponse<RegisterInterestOutputModel>()
                {
                    Error = "CR002001"
                };
            }

            if (accountDetails.IsCreator)
            {
                _logger.LogError("Account already a creator. Leaving function.");
                return new GenericResponse<RegisterInterestOutputModel>()
                {
                    Error = "CR002002"
                };
            }

            if (accountDetails.RestrictedStatus == 2)
            {
                _logger.LogError("Account is marked as fully restricted. Leaving function.");
                return new GenericResponse<RegisterInterestOutputModel>()
                {
                    Error = "CR002003"
                };
            }

            var existingRegisteredInterest = await _registeredInterestRepo.GetByAccountIdAsync(accountId);
            if (existingRegisteredInterest is not null)
            {
                if (existingRegisteredInterest.AccountId == accountId)
                {
                    _logger.LogDebug("Account requesting access already has an interest registered");
                    switch (existingRegisteredInterest.ApprovalStatus)
                    {
                        case (int)ApprovalStatusEnum.Requested:
                            _logger.LogDebug("Account registered interest is still at request status. Returning success.");
                            return new GenericResponse<RegisterInterestOutputModel>()
                            {
                                Data = new RegisterInterestOutputModel()
                                {
                                    SuccessfullyRegistered = true
                                }
                            };
                        case (int)ApprovalStatusEnum.Approved or (int)ApprovalStatusEnum.Denied:
                            _logger.LogDebug("Account has been approved access already or has been denied. Returning fail.");
                            return new GenericResponse<RegisterInterestOutputModel>()
                            {
                                Data = new RegisterInterestOutputModel()
                                {
                                    SuccessfullyRegistered = false
                                }
                            };
                    }
                }
            }

            _logger.LogDebug("Account has not yet registered interest.");

            var newRegisteredInterest = new RegisteredInterest()
            {
                Id = Guid.NewGuid().ToString(),
                AccountId = accountId,
                RequestMessage = registerInterestInputModel.RequestMessage,
                ApprovalStatus = (int)ApprovalStatusEnum.Requested,
                CreatedOn = DateTime.UtcNow
            };

            var registeredInterest = await _registeredInterestRepo.CreateAsync(newRegisteredInterest);
            if (registeredInterest is null)
            {
                _logger.LogError("Repo error during Insert of registered_interest record");
                return new GenericResponse<RegisterInterestOutputModel>()
                {
                    Data = new RegisterInterestOutputModel()
                    {
                        SuccessfullyRegistered = false
                    }
                };
            }

            _logger.LogDebug("Interest has been registered successfully");
            return new GenericResponse<RegisterInterestOutputModel>()
            {
                Data = new RegisterInterestOutputModel()
                {
                    SuccessfullyRegistered = true
                }
            };
        }
    }
}
