using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto?> GetTaskByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskDto>> GetTasksAsync(TaskListRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto?> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> BulkMarkAsDoneAsync(List<Guid> taskIds, Guid userId, CancellationToken cancellationToken = default);
}

