using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers;

[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ICheckoutService checkoutService, ILogger<CheckoutController> logger)
    {
        _checkoutService = checkoutService;
        _logger = logger;
    }

    [HttpPost("create-payment-intent")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreatePIInputModel createPIInputModel)
    {
        _logger.LogDebug("Entered CreateCheckoutSession end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _checkoutService.CreatePaymentIntent(collectorId, createPIInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("orderfrombasket")]
    public async Task<IActionResult> CreateNewOrderFromActiveBasket([FromBody] OrderFromBasketInputModel orderFromBasketInputModel)
    {
        _logger.LogDebug("Entered CreateNewOrderFromActiveBasket end point on CollectorApi.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _checkoutService.CreateNewOrderFromActiveBasket(collectorId, orderFromBasketInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("orderfromfail")]
    public async Task<IActionResult> CreateNewOrderFromFailedOrder([FromBody] OrderFromFailInputModel orderFromFailInputModel)
    {
        _logger.LogDebug("Entered CreateNewOrderFromFailedOrder end point on CollectorApi.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _checkoutService.CreateNewOrderFromFailedOrder(collectorId, orderFromFailInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }
}