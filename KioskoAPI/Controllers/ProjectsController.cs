using Microsoft.AspNetCore.Mvc;
using KioskoAPI.Services;
using KioskoAPI.Models;

namespace KioskoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public ProjectsController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        try
        {
            var projects = await _mongoDBService.GetProjects();
            return Ok(projects);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(string id)
    {
        try
        {
            var project = await _mongoDBService.GetProject(id);
            if (project == null)
            {
                return NotFound();
            }
            return Ok(project);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(string id, [FromBody] Project project)
    {
        try
        {
            if (id != project.Id)
            {
                return BadRequest("ID mismatch");
            }

            await _mongoDBService.UpdateProject(project);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
