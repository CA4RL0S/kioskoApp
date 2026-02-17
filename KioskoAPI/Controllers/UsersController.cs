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

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _mongoDBService.GetUsers();
            Console.WriteLine($"[DEBUG] GetUsers found {users.Count} users.");
            foreach(var u in users) Console.WriteLine($" - {u.Username}");
            return Ok(users);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error getting users: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyUser(string id)
    {
        try
        {
            await _mongoDBService.VerifyUser(id);
            return NoContent();
        }
        catch (Exception ex)
        {
             return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            await _mongoDBService.DeleteUser(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
