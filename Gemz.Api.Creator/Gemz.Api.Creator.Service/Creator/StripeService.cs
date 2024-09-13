using Gemz.Api.Creator.Data.Repository;
using Gemz.Api.Creator.Service.Creator.Model;
using Gemz.ServiceBus.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Gemz.Api.Creator.Service.Creator
{
    public class StripeService : IStripeService
    {
        private readonly IOptions<StripeConfig> _stripeConfig;
        private readonly IAccountRepository _accountRepo;
        private readonly IOptions<GemzDefaultsConfig> _gemzDefaultsConfig;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IOptions<StripeConfig> stripeConfig,
                            IAccountRepository accountRepo,
                            IOptions<GemzDefaultsConfig> gemzDefaultsConfig,
                            ILogger<StripeService> logger)
        {
            _stripeConfig = stripeConfig;
            _accountRepo = accountRepo;
            _gemzDefaultsConfig = gemzDefaultsConfig;
            _logger = logger;
        }

        public async Task<GenericResponse<OnboardingAccountLinkOutputModel>> CreateOnboardingAccountLink(string creatorId)
        {
            _logger.LogDebug("Entered CreateOnboardingAccountLink function.");

            if (string.IsNullOrEmpty(creatorId))
            {
                _logger.LogError("Missing or empty Creator Id. Leaving function.");
                return new GenericResponse<OnboardingAccountLinkOutputModel>()
                {
                    Error = "CR001400"
                };
            }

            var stripeApiKey = _stripeConfig.Value.ApiKey;
            if (string.IsNullOrEmpty(stripeApiKey))
            {
                _logger.LogError("Config has missing or empty Stripe Api Key. Leaving function.");
                return new GenericResponse<OnboardingAccountLinkOutputModel>()
                {
                    Error = "CR001401"
                };
            }

            var creatorAccount = await _accountRepo.GetAsync(creatorId);
            if (creatorAccount is null)
            {
                _logger.LogError("Repo unable to retrieve Creators Account. Leaving function.");
                return new GenericResponse<OnboardingAccountLinkOutputModel>()
                {
                    Error = "CR001406"
                };
            }

            if (creatorAccount.OnboardingStatus == (int)OnboardingStatusEnum.Complete)
            {
                _logger.LogError("Creator account already completed onboarded Stripe Account. Leaving function.");
                return new GenericResponse<OnboardingAccountLinkOutputModel>()
                {
                    Error = "CR001407"
                };
            }

            try
            {
                var stripeAccountId = creatorAccount.OnboardingStatus == (int)OnboardingStatusEnum.NotStarted ?
                        string.Empty : creatorAccount.StripeAccountId;

                if (creatorAccount.OnboardingStatus == (int)OnboardingStatusEnum.NotStarted)
                {
                    stripeAccountId = await CreateNewAccountOnStripe(creatorId, stripeApiKey);
                    if (string.IsNullOrEmpty(stripeAccountId))
                    {
                        _logger.LogError("No Account object returned from Stripe Create account call. Leaving function.");
                        return new GenericResponse<OnboardingAccountLinkOutputModel>()
                        {
                            Error = "CR001402"
                        };

                    }
                }

                var accountLinkData = await FetchStripeAccountOnboardingLink(stripeAccountId);

                if (accountLinkData is null)
                {
                    _logger.LogError("No AccountLink object returned from Stripe Create AccountLink call. Leaving function.");
                    return new GenericResponse<OnboardingAccountLinkOutputModel>()
                    {
                        Error = "CR001403"
                    };
                }

                if (string.IsNullOrEmpty(accountLinkData.Url))
                {
                    _logger.LogError("No URL from returned from Stripe Create AccountLink call. Leaving function.");
                    return new GenericResponse<OnboardingAccountLinkOutputModel>()
                    {
                        Error = "CR001404"
                    };
                }

                var accountOnboardingFieldsPatched = await _accountRepo.PatchOnboardingFields(creatorId, (int)OnboardingStatusEnum.Started,
                                                                                stripeAccountId,
                                                                                false,
                                                                                false);
                if (!accountOnboardingFieldsPatched)
                {
                    _logger.LogError("Error in Repo when trying to patch account onboarding fields. Leaving function.");
                    return new GenericResponse<OnboardingAccountLinkOutputModel>()
                    {
                        Error = "CR001405"
                    };
                }

                return new GenericResponse<OnboardingAccountLinkOutputModel>()
                {
                    Data = new OnboardingAccountLinkOutputModel()
                    {
                        OnboardingUrl = accountLinkData.Url
                    }
                };
            }
            catch (StripeException e)
            {
                _logger.LogError("Error occured trying to connect to Stripe API. Leaving function.");
                _logger.LogError($"Stripe Error: {e.StripeError.Error} | {e.StripeError.Message}");
                return new GenericResponse<OnboardingAccountLinkOutputModel>()
                {
                    Error = "CL001408"
                };
            }
        }

        public async Task UpdateCreatorAccountFromStripeEvent(StripeAccountMessageModel stripeAccountMessageModel)
        {
            _logger.LogDebug("Entered UpdateCreatorAccountFromStripeEvent in StripeService. Ending processing.");

            if (string.IsNullOrEmpty(stripeAccountMessageModel.StripeAccountId))
            {
                _logger.LogError("Stripe account.updated sent with no Stripe Account Id. Ending processing.");
                return;
            }

            if (string.IsNullOrEmpty(stripeAccountMessageModel.CreatorIdFromMetadata))
            {
                _logger.LogError("Stripe account.updated sent with no gemzId in Metadata. Ending processing.");
                return;
            }

            var creatorAccount = await _accountRepo.GetAsync(stripeAccountMessageModel.CreatorIdFromMetadata);
            if (creatorAccount is null)
            {
                _logger.LogError("Stripe account.updated gemzId that doesn't exist in Gemz DB. Ending processing.");
                return;
            }

            if (creatorAccount.StripeAccountId != stripeAccountMessageModel.StripeAccountId)
            {
                _logger.LogError("Stripe accountId does not match Creators Stripe account ID. Ending processing.");
                return;
            }

            if (!creatorAccount.StripeDetailsSubmitted && stripeAccountMessageModel.DetailsSubmitted)
            {
                var accountUpdated = await _accountRepo.PatchStripeAccountSettings(stripeAccountMessageModel.CreatorIdFromMetadata,
                    stripeAccountMessageModel.ChargesEnabled ? (int)OnboardingStatusEnum.Complete : creatorAccount.OnboardingStatus,
                    stripeAccountMessageModel.DetailsSubmitted,
                    stripeAccountMessageModel.ChargesEnabled,
                    stripeAccountMessageModel.ChargesEnabled ? _gemzDefaultsConfig.Value.DefaultCommissionPercentage : null);
                if (!accountUpdated)
                {
                    _logger.LogError("Repo error when trying to update Stripe fields in Creator Account. Leaving function.");
                }
                return;
            }

            if (!creatorAccount.StripeDetailsSubmitted && !stripeAccountMessageModel.DetailsSubmitted)
            {
                _logger.LogDebug("Ignoring Stripe account.updated as nothing of interest. Leaving function.");
                return;
            }

            if (stripeAccountMessageModel.DetailsSubmitted && stripeAccountMessageModel.ChargesEnabled != creatorAccount.StripeChargesEnabled)
            {
                var accountUpdated =
                    await _accountRepo.PatchStripeAccountSettings(stripeAccountMessageModel.CreatorIdFromMetadata,
                        stripeAccountMessageModel.ChargesEnabled ? (int)OnboardingStatusEnum.Complete : creatorAccount.OnboardingStatus,
                        true,
                        stripeAccountMessageModel.ChargesEnabled,
                        stripeAccountMessageModel.ChargesEnabled ? _gemzDefaultsConfig.Value.DefaultCommissionPercentage : null);
                if (!accountUpdated)
                {
                    _logger.LogError("Repo error when trying to update Stripe fields in Creator Account. Leaving function.");
                }
                return;
            }

            _logger.LogDebug("Ignoring Stripe account.updated as nothing of interest. Leaving function.");
            return;
        }

        public async Task<GenericResponse<AccountOnboardingStatusOutputModel>> CheckStripeOnboardingStatus(
            string creatorId)
        {
            _logger.LogDebug("Entrered CheckStripeOnboardingStatus function.");

            if (string.IsNullOrEmpty(creatorId))
            {
                _logger.LogError("creatorId is null or empty. Leaving function.");
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Error = "CL001900"
                };
            }

            var stripeApiKey = _stripeConfig.Value.ApiKey;
            if (string.IsNullOrEmpty(stripeApiKey))
            {
                _logger.LogError("Config has missing or empty Stripe Api Key. Leaving function.");
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Error = "CR001901"
                };
            }

            var creatorAccount = await _accountRepo.GetAsync(creatorId);
            if (creatorAccount is null)
            {
                _logger.LogError("Repo unable to retrieve Creators Account. Leaving function.");
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Error = "CR001902"
                };
            }

            if (creatorAccount.OnboardingStatus == (int)OnboardingStatusEnum.Complete)
            {
                _logger.LogDebug("Creator already fully onboarded in our system.");
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Data = new AccountOnboardingStatusOutputModel()
                    {
                        OnboardedFullyInStripe = true
                    }
                };
            }

            if (creatorAccount.OnboardingStatus == (int)OnboardingStatusEnum.NotStarted && string.IsNullOrEmpty(creatorAccount.StripeAccountId))
            {
                _logger.LogDebug("Account has not start onboarding process.");
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Data = new AccountOnboardingStatusOutputModel()
                    {
                        OnboardedFullyInStripe = false
                    }
                };
            }

            if (creatorAccount.OnboardingStatus == (int)OnboardingStatusEnum.Started &&
                string.IsNullOrEmpty(creatorAccount.StripeAccountId))
            {
                _logger.LogError("Creator has started onboarding but their Stripe Account Id blank. Leaving function.");
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Error = "CL001903"
                };
            }

            GenericResponse<bool> accountFullyOnboarded;

            try
            {

                accountFullyOnboarded = await AccountFullyOnboardedOnStripe(creatorAccount.StripeAccountId, stripeApiKey);
            }
            catch (StripeException e)
            {
                _logger.LogError("Error occured trying to connect to Stripe API. Leaving function.");
                _logger.LogError($"Stripe Error: {e.StripeError.Error} | {e.StripeError.Message}");
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Error = "CL001904"
                };
            }

            if (!string.IsNullOrEmpty(accountFullyOnboarded.Error))
            {
                return new GenericResponse<AccountOnboardingStatusOutputModel>()
                {
                    Error = accountFullyOnboarded.Error
                };
            }

            if (accountFullyOnboarded.Data)
            {
                _logger.LogDebug("Account fully onboarded in Stripe. Updating Creator Onboarding Status to Complete.");
                var accountUpdated =
                    await _accountRepo.PatchStripeAccountSettings(creatorId, (int)OnboardingStatusEnum.Complete,
                        true, 
                        true,
                        _gemzDefaultsConfig.Value.DefaultCommissionPercentage);
                if (!accountUpdated)
                {
                    _logger.LogError(
                        "Repo error when trying to update Stripe fields in Creator Account. Leaving function.");
                    return new GenericResponse<AccountOnboardingStatusOutputModel>()
                    {
                        Error = "CL001905"
                    };
                }
            }

            return new GenericResponse<AccountOnboardingStatusOutputModel>()
            {
                Data = new AccountOnboardingStatusOutputModel()
                {
                    OnboardedFullyInStripe = accountFullyOnboarded.Data
                }
            };
        }

        private async Task<string> CreateNewAccountOnStripe(string creatorId, string stripeApiKey)
        {
            StripeConfiguration.ApiKey = stripeApiKey;
            var accountOptions = new AccountCreateOptions
            {
                Type = "standard",
                Metadata = new Dictionary<string, string>()
                    {
                        {"gemzId",creatorId}
                    }
            };
            var accountService = new AccountService();

            _logger.LogInformation("Creating Account on Stripe");
            var stripeAccount = await accountService.CreateAsync(accountOptions);

            return stripeAccount?.Id;
        }

        private async Task<GenericResponse<bool>> AccountFullyOnboardedOnStripe(string stripeAccountId,
            string stripeApiKey)
        {
            var stripeAccountData = await FetchStripeAccountData(stripeAccountId, stripeApiKey);
            if (stripeAccountData is null)
            {
                _logger.LogError("No account data returned from Stripe. Leaving function.");
                return new GenericResponse<bool>()
                {
                    Error = "CL001408"
                };
            }

            if (stripeAccountData.DetailsSubmitted && stripeAccountData.ChargesEnabled)
            {
                _logger.LogDebug("Account status on Stripe is Fully Onboarded.");
                return new GenericResponse<bool>()
                {
                    Data = true
                };
            }

            _logger.LogDebug("Account Status on Stripe is partially onboarded.");
            return new GenericResponse<bool>()
            {
                Data = false
            };
        }

        private async Task<Account> FetchStripeAccountData(string stripeAccountId, string stripeApiKey)
        {
            _logger.LogDebug("Fetching Stripe Account to check onboarding status.");
            StripeConfiguration.ApiKey = stripeApiKey;
            var stripeAccountService = new AccountService();
            return await stripeAccountService.GetAsync(stripeAccountId);
        }

        private async Task<AccountLink> FetchStripeAccountOnboardingLink(string stripeAccountId)
        {
            var accountLinkCreateOptions = new AccountLinkCreateOptions
            {
                Account = stripeAccountId,
                RefreshUrl = _stripeConfig.Value.RefreshUrl,
                ReturnUrl = _stripeConfig.Value.ReturnUrl,
                Type = "account_onboarding"
            };

            StripeConfiguration.ApiKey = _stripeConfig.Value.ApiKey;

            var accountLinkService = new AccountLinkService();

            _logger.LogInformation("Calling Stripe AccountLinkService.");
            return await accountLinkService.CreateAsync(accountLinkCreateOptions);
        }
    }
}
