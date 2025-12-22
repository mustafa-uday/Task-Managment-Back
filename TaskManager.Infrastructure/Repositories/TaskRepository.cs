using Microsoft.EntityFrameworkCore;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Task> CreateAsync(Domain.Entities.Task task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<Domain.Entities.Task?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.Id == id && t.OwnerUserId == userId && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<Domain.Entities.Task>> GetTasksAsync(TaskListRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Where(t => t.OwnerUserId == userId && !t.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
        }

        if (request.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= request.DueDateFrom.Value);
        }

        if (request.DueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= request.DueDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Title.Contains(request.Search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var isDescending = request.SortDirection?.ToLower() == "desc";
            query = request.SortBy.ToLower() switch
            {
                "title" => isDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "status" => isDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "priority" => isDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                "duedate" => isDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                "createdat" => isDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedAt);
        }

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Domain.Entities.Task>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<Domain.Entities.Task?> UpdateAsync(Domain.Entities.Task task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await GetByIdAsync(id, userId, cancellationToken);
        if (task == null)
            return false;

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<Domain.Entities.Task>> GetTasksByIdsAsync(List<Guid> taskIds, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => taskIds.Contains(t.Id) && t.OwnerUserId == userId && !t.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}

