using TaskManager.Domain.Entities;

namespace TaskManager.Application.DTOs.Tasks;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Domain.Entities.TaskStatus Status { get; set; }
    public Domain.Entities.TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid OwnerUserId { get; set; }
}

