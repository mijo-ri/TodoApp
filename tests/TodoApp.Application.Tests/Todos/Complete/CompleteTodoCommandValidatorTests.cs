using FluentAssertions;
using TodoApp.Application.Todos.Complete;

namespace TodoApp.Application.Tests.Todos.Complete;
public class CompleteTodoCommandValidatorTests
{
    private readonly CompleteTodoCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenIdIsNotEmpty()
    {
        // Arrange
        var command = new CompleteTodoCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        var command = new CompleteTodoCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
