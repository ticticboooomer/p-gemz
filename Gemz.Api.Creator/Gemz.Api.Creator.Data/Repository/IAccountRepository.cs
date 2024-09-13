using Gemz.Api.Creator.Data.Model;

namespace Gemz.Api.Creator.Data.Repository
{
    public interface IAccountRepository
    {
        Task<Account> GetAsync(string id);

        Task<bool> PatchOnboardingFields(string creatorId, int onboardingStatus, string stripeAccountId, bool detailsSubmitted,
            bool chargesEnabled);

        Task<bool> PatchStripeAccountSettings(string creatorId, int onboardingStatus, bool detailsSubmitted, bool chargesEnabled,
            int? commissionPercentage);
    }
}
