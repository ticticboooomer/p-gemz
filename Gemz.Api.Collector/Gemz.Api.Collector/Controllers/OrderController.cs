using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost("order")]
    public async Task<IActionResult> FetchOrderById([FromBody] OrderIdModel orderIdModel)
    {
        _logger.LogDebug("Entered FetchOrderById end point on CollectorApi.");

        if (HttpContext?.Items != null)
        {
            var accountParam = HttpContext.Items["Account"];
            if (accountParam != null)
            {
                var collectorId = accountParam.ToString();
                if (collectorId != null)
                {
                    var res = await _orderService.FetchOrderById(collectorId, orderIdModel);
                    _logger.LogDebug($"Returned from Service function.");
                    return Ok(res);
                }
            }
        }
        return BadRequest();
    }

    [HttpPost("list")]
    public async Task<IActionResult> FetchOrderList()
    {
        _logger.LogDebug("Entered FetchOrderList end point on CollectorApi.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _orderService.FetchOrderList(collectorId);
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

    [HttpPost("orderbypis")]
    public async Task<IActionResult> FetchOrderByPaymentIntentSecret([FromBody] PaymentIntentInputModel paymentIntentInputModel)
    {
        _logger.LogDebug("Entered FetchOrderByPaymentIntentSecret end point on CollectorApi.");

        if (HttpContext?.Items != null)
        {
            var accountParam = HttpContext.Items["Account"];
            if (accountParam != null)
            {
                var collectorId = accountParam.ToString();
                if (collectorId != null)
                {
                    var res = await _orderService.FetchOrderByPaymentIntentSecret(collectorId, paymentIntentInputModel);
                    _logger.LogDebug($"Returned from Service function.");
                    return Ok(res);
                }
            }
        }
        return BadRequest();
    }

    [HttpPost("updatestatus")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusInputModel orderStatusInputModel)
    {
        _logger.LogDebug("Entered UpdateOrderStatus end point on CollectorApi.");

        if (HttpContext?.Items != null)
        {
            var accountParam = HttpContext.Items["Account"];
            if (accountParam != null)
            {
                var collectorId = accountParam.ToString();
                if (collectorId != null)
                {
                    var res = await _orderService.UpdateOrderStatus(collectorId, orderStatusInputModel);
                    _logger.LogDebug($"Returned from Service function.");
                    return Ok(res);
                }
            }
        }
        return BadRequest();
    }

    [HttpPost("paymentpending")]
    public async Task<IActionResult> UpdateOrderForPaymentPending([FromBody] PaymentPendingInputModel paymentPendingInputModel)
    {
        _logger.LogDebug("Entered UpdateOrderForPaymentPending end point on CollectorApi.");

        if (HttpContext?.Items != null)
        {
            var accountParam = HttpContext.Items["Account"];
            if (accountParam != null)
            {
                var collectorId = accountParam.ToString();
                if (collectorId != null)
                {
                    var res = await _orderService.UpdateOrderForPaymentPending(collectorId, paymentPendingInputModel);
                    _logger.LogDebug($"Returned from Service function.");
                    return Ok(res);
                }
            }
        }
        return BadRequest();
    }

    [HttpPost("pagedlist")]
    public async Task<IActionResult> FetchOrderListPaged([FromBody] OrderListPagedInputModel orderListPagedInputModel)
    {
        _logger.LogDebug("Entered FetchOrderListPaged end point on CollectorApi.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _orderService.FetchOrderListPaged(collectorId, orderListPagedInputModel);
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
    }

}