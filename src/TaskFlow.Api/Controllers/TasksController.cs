using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? throw new UnauthorizedAccessException("Missing user id claim."));

    /// <summary>List my tasks with optional filtering, search and paging.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskResponse>>> GetAll([FromQuery] TaskQuery query)
        => Ok(await _taskService.GetAllAsync(CurrentUserId, query));

    /// <summary>Get a single task by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
        => Ok(await _taskService.GetByIdAsync(CurrentUserId, id));

    /// <summary>Create a new task.</summary>
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var created = await _taskService.CreateAsync(CurrentUserId, request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing task.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request)
        => Ok(await _taskService.UpdateAsync(CurrentUserId, id, request));

    /// <summary>Delete a task.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(CurrentUserId, id);
        return NoContent();
    }
}
