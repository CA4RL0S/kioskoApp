using Microsoft.AspNetCore.Mvc;
using KioskoAPI.Services;

namespace KioskoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly CloudinaryService _cloudinaryService;

    public UploadController(CloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            var url = await _cloudinaryService.UploadFile(file);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
