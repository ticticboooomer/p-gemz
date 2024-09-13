using Gemz.Api.Collector.Middleware;
using Gemz.Api.Collector.Service.Collector;
using Gemz.Api.Collector.Service.Collector.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Collector.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    
    [AllowUnauthorized]
    [HttpGet("image/{id:guid}")]
    public async Task<IActionResult> FetchImageFromStorage(Guid id)
    {
        _logger.LogDebug("Entered FetchImageFromStorage controller end point");
        var result = await _imageService.FetchImageFromStorage(id.ToString());

        if (result is null)
        {
            return NotFound();
        }
        return File(result.Content, "image/png", result.Name);
    }
}