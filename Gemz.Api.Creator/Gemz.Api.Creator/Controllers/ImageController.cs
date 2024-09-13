using Gemz.Api.Creator.Middleware;
using Gemz.Api.Creator.Service.Creator;
using Gemz.Api.Creator.Service.Creator.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gemz.Api.Creator.Controllers;

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

    [HttpPost("page")]
    public async Task<IActionResult> FetchImagesForCreator([FromBody] ImagesPagingModel imagesPagingModel)
    {
        _logger.LogDebug("Entered FetchImagesForCreator end point on Creator Api.");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var res = await _imageService.FetchPageOfImages(imagesPagingModel, creatorId);
        
        _logger.LogDebug($"Returned from Service function.");

        return Ok(res);
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
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImages(IFormFile[] files)
    {
        _logger.LogDebug("Entered UploadImages controller end point");
        
        var creatorId = HttpContext.Items["Account"].ToString();
        var result = await _imageService.UploadImagesToStorage(files, creatorId);
        
        _logger.LogDebug("Returned from Image Service");
        return Ok(result);
    }

    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveImage(ImageIdModel imageIdModel)
    {
        _logger.LogDebug("Entered ArchiveImage controller end point");
        var creatorId = HttpContext.Items["Account"].ToString();
        
        var result = await _imageService.ArchiveImage(imageIdModel, creatorId);

        _logger.LogDebug("Returned from Image Service");
        return Ok(result);
    }
}