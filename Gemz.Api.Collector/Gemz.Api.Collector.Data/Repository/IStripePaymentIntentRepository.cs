using Gemz.Api.Collector.Data.Model;
using Stripe;

namespace Gemz.Api.Collector.Data.Repository;

public interface IStripePaymentIntentRepository
{
    Task<StripePaymentIntent> CreateAsync(StripePaymentIntent stripePaymentIntent);
}