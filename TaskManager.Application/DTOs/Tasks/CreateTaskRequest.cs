using TaskManager.Domain.Entities;

namespace TaskManager.Application.DTOs.Tasks;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Domain.Entities.TaskStatus Status { get; set; } = Domain.Entities.TaskStatus.Todo;
    public Domain.Entities.TaskPriority Priority { get; set; } = Domain.Entities.TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
}

