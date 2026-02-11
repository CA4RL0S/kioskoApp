using Microsoft.AspNetCore.Mvc;
using KioskoAPI.Services;
using KioskoAPI.Models;

namespace KioskoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public ActivitiesController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetActivities(string userId)
    {
        try
        {
            var activities = await _mongoDBService.GetActivitiesByUser(userId);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateActivity([FromBody] Activity activity)
    {
        try
        {
            activity.Timestamp = DateTime.UtcNow;
            await _mongoDBService.CreateActivity(activity);
            return Ok(activity);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
