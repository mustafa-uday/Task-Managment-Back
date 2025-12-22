using TaskManager.Domain.Entities;

namespace TaskManager.Application.DTOs.Tasks;

public class TaskListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
    public Domain.Entities.TaskStatus? Status { get; set; }
    public Domain.Entities.TaskPriority? Priority { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? Search { get; set; }
}

