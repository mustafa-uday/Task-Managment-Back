using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;

namespace TaskManager.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request.";
        var errors = new Dictionary<string, string[]>();
        var isDevelopment = _environment.IsDevelopment();

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed";
                errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
        }

        // Check for database connection errors
        var innerException = exception.InnerException;
        var exceptionMessage = exception.Message;
        var innerMessage = innerException?.Message ?? "";
        
        if (exceptionMessage.Contains("transient failure") || 
            exceptionMessage.Contains("Connection refused") ||
            exceptionMessage.Contains("Failed to connect") ||
            innerMessage.Contains("Connection refused") ||
            innerMessage.Contains("Failed to connect") ||
            innerMessage.Contains("does not exist") ||
            exceptionMessage.Contains("does not exist"))
        {
            statusCode = HttpStatusCode.ServiceUnavailable;
            if (innerMessage.Contains("does not exist") || exceptionMessage.Contains("does not exist"))
            {
                message = "Database 'TaskManagerDb' does not exist. Please create it first. See CREATE_DATABASE.md for instructions.";
            }
            else
            {
                message = "Database connection failed. Please ensure PostgreSQL is running and the database exists.";
            }
        }
        
        // In development, include more details
        if (isDevelopment && statusCode == HttpStatusCode.InternalServerError)
        {
            message = $"{message} Error: {exception.Message}";
            if (innerException != null)
            {
                message += $" Inner: {innerException.Message}";
            }
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            traceId,
            message,
            errors
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        });

        return context.Response.WriteAsync(jsonResponse);
    }
}

