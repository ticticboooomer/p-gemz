using Azure.Messaging.ServiceBus;
using Gemz.Api.Creator.Model;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Options;
using Stripe.Climate;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Gemz.Api.Creator.Service.Creator;
using MassTransit;
using Stripe;
using Gemz.ServiceBus.Model;
using Gemz.ServiceBus.Factory;

namespace Gemz.Api.Creator.Services
{
    public class StripeAccountConsumer : IConsumer<StripeAccountMessageModel>
    {
        private readonly ILogger<StripeAccountConsumer> _logger;
        private readonly IStripeService _stripeService;


        public StripeAccountConsumer(ILogger<StripeAccountConsumer> logger, IOptions<ServiceBusConfig> serviceBusConfig, IStripeService stripeService)
        {
            _logger = logger;
            _stripeService = stripeService;
        }

        public async Task Consume(ConsumeContext<StripeAccountMessageModel> context)
        {
            _logger.LogDebug("Entered Stripe Account Consumer");
            await _stripeService.UpdateCreatorAccountFromStripeEvent(context.Message);
        }
    }
}
