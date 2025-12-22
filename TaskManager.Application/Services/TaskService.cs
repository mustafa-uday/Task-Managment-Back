using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = new Domain.Entities.Task
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate.HasValue ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc) : null,
            OwnerUserId = userId
        };

        var createdTask = await _taskRepository.CreateAsync(task, cancellationToken);
        return MapToDto(createdTask);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId, cancellationToken);
        return task == null ? null : MapToDto(task);
    }

    public async Task<PagedResult<TaskDto>> GetTasksAsync(TaskListRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var pagedResult = await _taskRepository.GetTasksAsync(request, userId, cancellationToken);
        return new PagedResult<TaskDto>
        {
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<TaskDto?> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, userId, cancellationToken);
        if (task == null)
            return null;

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate.HasValue ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc) : null;

        var updatedTask = await _taskRepository.UpdateAsync(task, cancellationToken);
        return updatedTask != null ? MapToDto(updatedTask) : null;
    }

    public async Task<bool> DeleteTaskAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.DeleteAsync(id, userId, cancellationToken);
    }

    public async Task<bool> BulkMarkAsDoneAsync(List<Guid> taskIds, Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetTasksByIdsAsync(taskIds, userId, cancellationToken);
        if (tasks.Count != taskIds.Count)
            return false;

        foreach (var task in tasks)
        {
            task.Status = Domain.Entities.TaskStatus.Done;
            await _taskRepository.UpdateAsync(task, cancellationToken);
        }

        return true;
    }

    private static TaskDto MapToDto(Domain.Entities.Task task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            OwnerUserId = task.OwnerUserId
        };
    }
}

