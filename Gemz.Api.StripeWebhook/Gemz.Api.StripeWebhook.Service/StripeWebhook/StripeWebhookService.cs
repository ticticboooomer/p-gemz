using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Gemz.ServiceBus.Model;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Gemz.Api.StripeWebhook.Service.StripeWebhook
{
    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly IOptions<ServiceBusConfig> _serviceBusConfig;
        private readonly ILogger<StripeWebhookService> _logger;
        private readonly ISendEndpointProvider _endpointProvider;

        public StripeWebhookService(IOptions<ServiceBusConfig> serviceBusConfig, ILogger<StripeWebhookService> logger,
            ISendEndpointProvider endpointProvider)
        {
            _serviceBusConfig = serviceBusConfig;
            _logger = logger;
            _endpointProvider = endpointProvider;
        }

        public async Task SendOrderStatusUpdateToServiceBus(PaymentIntent paymentIntent, string eventType)
        {
            _logger.LogDebug("Entered SendOrderStatusUpdateToServiceBus function");

            var sender =
                await _endpointProvider.GetSendEndpoint(
                    new Uri("queue:" + _serviceBusConfig.Value.StripeHandlerCollectorQueueName));

            var model = new PaymentIntentMessageModel
            {
                PaymentIntentId = paymentIntent.Id,
                EventType = eventType,
                Amount = paymentIntent.Amount,
                ApplicationFeeAmount = paymentIntent.ApplicationFeeAmount ?? 0,
                Currency = paymentIntent.Currency,
                LatestChargeId = paymentIntent.LatestChargeId,
                PaymentMethodId = paymentIntent.PaymentMethodId,
                PaymentIntentClientSecret = paymentIntent.ClientSecret,
                CreatedOn = paymentIntent.Created
            };

            var metaInfo = paymentIntent.Metadata;

            if (metaInfo is null)
            {
                _logger.LogError("Metadata information is missing from the Stripe PaymentIntent object");
                model.MetadataCollectorId = string.Empty;
                model.MetadataOrderId = string.Empty;
            }
            else
            {
                if (!metaInfo.ContainsKey("orderId"))
                {
                    _logger.LogError("Stripe Payment Intent Object is missing orderId metadata");
                    model.MetadataOrderId = string.Empty;
                }
                else
                {
                    model.MetadataOrderId = metaInfo["orderId"];
                }
                if (!metaInfo.ContainsKey("collectorId"))
                {
                    _logger.LogError("Stripe Payment Intent Object is missing collectorId metadata");
                    model.MetadataCollectorId = string.Empty;
                }
                else
                {
                    model.MetadataCollectorId = metaInfo["collectorId"];
                }
            }

            await sender.Send(model);
            _logger.LogDebug("Message sent to service bus collector queue");
        }

        public async Task SendAccountUpdateToServiceBus(Account accountUpdated)
        {
            _logger.LogDebug("Entered SendAccountUpdateToServiceBus function");

            var sender =
                await _endpointProvider.GetSendEndpoint(
                    new Uri("queue:" + _serviceBusConfig.Value.StripeHandlerCreatorQueueName));
            var creatorIfFromMetadata = accountUpdated.Metadata.ContainsKey("gemzId")
                ? accountUpdated.Metadata["gemzId"]
                : string.Empty;

            var model = new StripeAccountMessageModel
            {
                StripeAccountId = accountUpdated.Id,
                ChargesEnabled = accountUpdated.ChargesEnabled,
                DetailsSubmitted = accountUpdated.DetailsSubmitted,
                CreatorIdFromMetadata = creatorIfFromMetadata
            };

            await sender.Send(model);
            _logger.LogDebug("Message sent to service bus creator queue");
        }
    }
}