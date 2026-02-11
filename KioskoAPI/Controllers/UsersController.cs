using Microsoft.AspNetCore.Mvc;
using KioskoAPI.Services;
using KioskoAPI.Models;

namespace KioskoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public UsersController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
    {
        try
        {
            if (id != user.Id)
            {
                return BadRequest("ID mismatch");
            }

            await _mongoDBService.UpdateUser(user);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/image")]
    public async Task<IActionResult> UpdateUserProfileImage(string id, [FromBody] string imageUrl)
    {
        try
        {
            await _mongoDBService.UpdateUserProfileImage(id, imageUrl);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
