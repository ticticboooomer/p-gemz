using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers;

[ApiController]
[Route("api/basket")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketService basketService, ILogger<BasketController> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    [HttpPost("getall")]
    public async Task<IActionResult> GetAllActiveBasketsForOneCollector()
    {
        _logger.LogDebug("Entered GetAllActiveBasketsForOneCollector end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.GetAllActiveBasketsForOneCollector(collectorId);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("getallpage")]
    public async Task<IActionResult> GetAllActiveBasketsForOneCollectorPaged([FromBody] GetAllBasketsPageInputModel getAllBasketsPageInputModel)
    {
        _logger.LogDebug("Entered GetAllActiveBasketsForOneCollectorPaged end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.GetAllActiveBasketsForOneCollectorPaged(collectorId, getAllBasketsPageInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }


    [HttpPost("getbyid")]
    public async Task<IActionResult> GetActiveBasketForCollectorById([FromBody] BasketGetByIdInputModel basketGetByIdInputModel)
    {
        _logger.LogDebug("Entered GetActiveBasketForCollectorById end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.GetActiveBasketForCollectorById(collectorId, basketGetByIdInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("getbytag")]
    public async Task<IActionResult> GetActiveBasketForCollectorByStoreTag([FromBody] BasketGetByStoreTagInputModel basketGetByStoreTagInputModel)
    {
        _logger.LogDebug("Entered GetActiveBasketForCollectorByStoreTag end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.GetActiveBasketForCollectorStoreTag(collectorId, basketGetByStoreTagInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }


    [HttpPost("add")]
    public async Task<IActionResult> AddItemToBasket([FromBody] BasketItemAddModel basketItemAddModel)
    {
        _logger.LogDebug("Entered AddItemToBasket end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.AddItemToBasket(collectorId, basketItemAddModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveItemFromBasket([FromBody] BasketItemRemoveModel basketItemRemoveModel)
    {
        _logger.LogDebug("Entered RemoveItemFromBasket end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.RemoveItemFromBasket(collectorId, basketItemRemoveModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("empty")]
    public async Task<IActionResult> EmptyBasket([FromBody] EmptyBasketInputModel emptyBasketInputModel)
    {
        _logger.LogDebug("Entered EmptyBasket end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.EmptyBasket(collectorId, emptyBasketInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateBasket([FromBody] BasketItemUpdateModel basketItemUpdateModel)
    {
        _logger.LogDebug("Entered UpdateBasket end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.UpdateItemInBasket(collectorId, basketItemUpdateModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("getitemdata")]
    public async Task<IActionResult> GetBasketItemData([FromBody] BasketItemInputModel basketItemInputModel)
    {
        _logger.LogDebug("Entered UpdateBasket end point.");

        var collectorId = HttpContext.Items["Account"]?.ToString();
        if (collectorId == null)
        {
            return BadRequest();
        }

        var res = await _basketService.GetBasketItemData(basketItemInputModel);
        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

    [HttpPost("getpaymentacc")]
    public async Task<IActionResult> GetPaymentAccountForBasket([FromBody] PaymentAccountInputModel paymentAccountInputModel)
    {
        _logger.LogDebug("Entered GetPaymentAccountForBasket end point.");

        var res = await _basketService.GetPaymentAccountByBasketId(paymentAccountInputModel);

        _logger.LogDebug($"Returned from Service function.");
        return Ok(res);
    }

}