using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Infrastructure.Data;

namespace TaskManager.IntegrationTests;

public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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

    [Fact]
    public async System.Threading.Tasks.Task Register_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Token);
        Assert.Equal(request.Email, authResponse.Email);
    }

    [Fact]
    public async System.Threading.Tasks.Task Register_DuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"test{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = email,
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        // Register first time
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Try to register again with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async System.Threading.Tasks.Task Login_ValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "password123";

        // Register user first
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Login
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Token);
        Assert.Equal(email, authResponse.Email);
    }

    [Fact]
    public async System.Threading.Tasks.Task Login_InvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

