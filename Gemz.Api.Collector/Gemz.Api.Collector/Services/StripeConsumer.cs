using Gemz.Api.Collector.Service.Collector;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MassTransit;
using Gemz.ServiceBus.Model;

namespace Gemz.Api.Collector.Services
{
    public class StripeConsumer : IConsumer<PaymentIntentMessageModel>
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<StripeConsumer> _logger;

        public StripeConsumer(IOrderService orderService,
            ILogger<StripeConsumer> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }


        public async Task Consume(ConsumeContext<PaymentIntentMessageModel> context)
        {
            _logger.LogDebug("Stripe Consumer Entered");

            var paymentIntentMessage = context.Message;
            await _orderService.UpdateOrderStatusFromStripeEvent(paymentIntentMessage);
        }
    }
}