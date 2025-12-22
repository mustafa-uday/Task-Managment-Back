using FluentValidation;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Validators;

namespace TaskManager.Tests.Validators;

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator;

    public UpdateTaskRequestValidatorTests()
    {
        _validator = new UpdateTaskRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new UpdateTaskRequest
        {
            Title = "Updated Task",
            Description = "Updated Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldFail()
    {
        // Arrange
        var request = new UpdateTaskRequest
        {
            Title = "",
            Description = "Updated Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_TitleTooShort_ShouldFail()
    {
        // Arrange
        var request = new UpdateTaskRequest
        {
            Title = "AB", // Less than 3 characters
            Description = "Updated Description"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }
}
