using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Invalid user ID");
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.CreateTaskAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.GetTaskByIdAsync(id, userId, cancellationToken);
        
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        return Ok(task);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks([FromQuery] TaskListRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.GetTasksAsync(request, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.UpdateTaskAsync(id, request, userId, cancellationToken);
        
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var deleted = await _taskService.DeleteTaskAsync(id, userId, cancellationToken);
        
        if (!deleted)
        {
            return NotFound(new { message = "Task not found" });
        }

        return NoContent();
    }

    [HttpPost("bulk-mark-done")]
    public async Task<ActionResult> BulkMarkAsDone([FromBody] BulkMarkDoneRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var success = await _taskService.BulkMarkAsDoneAsync(request.TaskIds, userId, cancellationToken);
        
        if (!success)
        {
            return BadRequest(new { message = "One or more tasks were not found" });
        }

        return Ok(new { message = "Tasks marked as done successfully" });
    }
}

public class BulkMarkDoneRequest
{
    public List<Guid> TaskIds { get; set; } = new();
}

