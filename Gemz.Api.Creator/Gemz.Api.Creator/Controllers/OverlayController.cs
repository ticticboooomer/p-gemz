using Gemz.Api.Creator.Service.Creator;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers;

[ApiController]
[Route("api/overlay")]
public class OverlayController : ControllerBase
{
    private readonly IOverlayService _service;

    public OverlayController(IOverlayService service)
    {
        _service = service;
    }


    [HttpPost("test/order")]
    public async Task<IActionResult> TestOrder()
    {
        var id = HttpContext.Items["Account"] as string;
        var res = await _service.SendOverlayTestOrder(id);
        return Ok(res);
    }

    [HttpPost("keys/all")]
    public async Task<IActionResult> GetAllKeys()
    {
        var id = HttpContext.Items["Account"] as string;
        var res = await _service.GetOverlayKeysForCreator(id);
        return Ok(res);
    }

    [HttpPost("keys/create")]
    public async Task<IActionResult> CreateKey()
    {
        var id = HttpContext.Items["Account"] as string;
        var res = await _service.CreateOverlayKey(id);
        return Ok(res);
    }

    [HttpPost("keys/revoke/{id}")]
    public async Task<IActionResult> RevokeKey([FromRoute]string id)
    {
        var cid = HttpContext.Items["Account"] as string;
        var res = await _service.RevokeKey(cid, id);

        return Ok(res);
    }
}
