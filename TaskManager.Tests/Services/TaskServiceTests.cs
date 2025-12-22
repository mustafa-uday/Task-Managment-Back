using Moq;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;

namespace TaskManager.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _taskService = new TaskService(_taskRepositoryMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTaskAsync_ValidRequest_ShouldReturnTaskDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = Domain.Entities.TaskStatus.Todo,
            Priority = Domain.Entities.TaskPriority.High
        };

        var taskId = Guid.NewGuid();
        var createdTask = new Domain.Entities.Task
        {
            Id = taskId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            OwnerUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Domain.Entities.Task>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateTaskAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(userId, result.OwnerUserId);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskByIdAsync_TaskExists_ShouldReturnTaskDto()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new Domain.Entities.Task
        {
            Id = taskId,
            Title = "Test Task",
            OwnerUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTaskByIdAsync_TaskNotFound_ShouldReturnNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Task?)null);

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId, userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTaskAsync_TaskExists_ShouldReturnUpdatedTaskDto()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTaskRequest
        {
            Title = "Updated Task",
            Description = "Updated Description",
            Status = Domain.Entities.TaskStatus.InProgress,
            Priority = Domain.Entities.TaskPriority.High
        };

        var existingTask = new Domain.Entities.Task
        {
            Id = taskId,
            Title = "Original Task",
            OwnerUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Task>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        // Act
        var result = await _taskService.UpdateTaskAsync(taskId, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Description, result.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTaskAsync_TaskExists_ShouldReturnTrue()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _taskRepositoryMock.Setup(x => x.DeleteAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, userId);

        // Assert
        Assert.True(result);
    }
}

