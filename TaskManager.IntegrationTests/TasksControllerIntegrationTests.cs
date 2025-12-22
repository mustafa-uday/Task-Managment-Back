using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.IntegrationTests;

public class TasksControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;

    public TasksControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        
        // Ensure database is created
        var dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _client?.Dispose();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "password123";

        // Register user
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return authResponse!.Token;
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = Domain.Entities.TaskStatus.Todo,
            Priority = Domain.Entities.TaskPriority.High
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var task = JsonSerializer.Deserialize<TaskDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(task);
        Assert.Equal(request.Title, task.Title);
        Assert.Equal(request.Description, task.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTasks_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTasks_WithAuth_ShouldReturnOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a task first
        var createRequest = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = Domain.Entities.TaskStatus.Todo,
            Priority = Domain.Entities.TaskPriority.High
        };
        await _client.PostAsJsonAsync("/api/tasks", createRequest);

        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<TaskDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(result);
        Assert.True(result.Items.Count > 0);
    }
}

