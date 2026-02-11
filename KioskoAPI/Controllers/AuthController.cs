using Microsoft.AspNetCore.Mvc;
using KioskoAPI.Services;
using KioskoAPI.Models;

namespace KioskoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public AuthController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _mongoDBService.Login(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        try
        {
            var createdUser = await _mongoDBService.CreateUser(user);
            return Ok(createdUser);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
