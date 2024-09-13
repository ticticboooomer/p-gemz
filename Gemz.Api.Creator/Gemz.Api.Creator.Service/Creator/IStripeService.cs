using Gemz.Api.Creator.Service.Creator.Model;
using Gemz.ServiceBus.Model;
using Stripe;

namespace Gemz.Api.Creator.Service.Creator
{
    public interface IStripeService
    {
        Task<GenericResponse<OnboardingAccountLinkOutputModel>> CreateOnboardingAccountLink(string creatorId);

        Task UpdateCreatorAccountFromStripeEvent(StripeAccountMessageModel stripeAccountMessageModel);
        Task<GenericResponse<AccountOnboardingStatusOutputModel>> CheckStripeOnboardingStatus(string creatorId);
    }
}
