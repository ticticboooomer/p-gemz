using Stripe;

namespace Gemz.Api.StripeWebhook.Service.StripeWebhook
{
    public interface IStripeWebhookService
    {
        Task SendOrderStatusUpdateToServiceBus(PaymentIntent paymentIntent, string eventType);

        Task SendAccountUpdateToServiceBus(Account accountUpdated);
    }
}
