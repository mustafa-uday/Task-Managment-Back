using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<Domain.Entities.Task> CreateAsync(Domain.Entities.Task task, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Task?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<Domain.Entities.Task>> GetTasksAsync(TaskListRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Task?> UpdateAsync(Domain.Entities.Task task, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Task>> GetTasksByIdsAsync(List<Guid> taskIds, Guid userId, CancellationToken cancellationToken = default);
}

