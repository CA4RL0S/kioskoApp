using Microsoft.AspNetCore.Mvc;
using KioskoAPI.Services;
using KioskoAPI.Models;

namespace KioskoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public NotificationsController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    /// <summary>
    /// Get all notifications for a student by their matrícula.
    /// </summary>
    [HttpGet("{matricula}")]
    public async Task<IActionResult> GetNotifications(string matricula)
    {
        try
        {
            var notifications = await _mongoDBService.GetNotificationsByMatricula(matricula);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new notification (called by EvaluatorApp after evaluation).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] Notification notification)
    {
        try
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            await _mongoDBService.CreateNotification(notification);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        try
        {
            await _mongoDBService.MarkNotificationAsRead(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
