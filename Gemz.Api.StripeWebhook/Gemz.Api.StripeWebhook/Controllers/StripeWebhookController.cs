using Gemz.Api.StripeWebhook.Service.StripeWebhook;
using Gemz.Api.StripeWebhook.Service.StripeWebhook.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace Gemz.Api.StripeWebhook.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IOptions<StripeConfig> _stripeConfig;
        private readonly IStripeWebhookService _stripeWebhookService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(IOptions<StripeConfig> stripeConfig,
                                        ILogger<StripeWebhookController> logger, IStripeWebhookService stripeWebhookService)
        {
            _stripeConfig = stripeConfig;
            _logger = logger;
            _stripeWebhookService = stripeWebhookService;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var webhookEndpointSecret = _stripeConfig.Value.WebHookEndpointSecret;
            if (string.IsNullOrEmpty(webhookEndpointSecret))
            {
                _logger.LogWarning("Webhook Endpoint Secret missing from config. Aborting.");
                return BadRequest();
            }

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], webhookEndpointSecret);

                switch (stripeEvent.Type)
                {
                    case Events.AccountUpdated:
                    {
                        var accountUpdated = stripeEvent.Data.Object as Account;
                        if (accountUpdated is not null)
                        {
                            await _stripeWebhookService.SendAccountUpdateToServiceBus(accountUpdated);
                        }
                        _logger.LogDebug("Handler Stripe Event AccountUpdated.");
                        return Ok();
                    }
                    case Events.PaymentIntentPaymentFailed:
                    {
                        var paymentIntentFailed = stripeEvent.Data.Object as PaymentIntent;
                        if (paymentIntentFailed != null)
                        {
                            await _stripeWebhookService.SendOrderStatusUpdateToServiceBus(paymentIntentFailed, stripeEvent.Type);
                        }
                        _logger.LogDebug("Handler Stripe Event PaymentIntentPaymentFailed.");
                        return Ok();
                    }
                    case Events.PaymentIntentSucceeded:
                    {
                        var paymentIntentSuccess = stripeEvent.Data.Object as PaymentIntent;
                        if (paymentIntentSuccess != null)
                        {
                            await _stripeWebhookService.SendOrderStatusUpdateToServiceBus(paymentIntentSuccess, stripeEvent.Type);
                        }
                        _logger.LogDebug("Handler Stripe Event PaymentIntentSuccess.");
                        return Ok();
                    }
                    default:
                        _logger.LogWarning("Unhandled event type: {0}", stripeEvent.Type);
                        return Ok();
                }
            }
            catch (StripeException e)
            {
                _logger.LogInformation($"Stripe Exception: {e}");
                return BadRequest();
            }

        }
    }
}
