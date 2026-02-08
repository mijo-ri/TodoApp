using FluentAssertions;
using TodoApp.Application.Todos.Reopen;

namespace TodoApp.Application.Tests.Todos.Reopen;

public class ReopenTodoCommandValidatorTests
{
    private readonly ReopenTodoCommandValidator _sut;

    public ReopenTodoCommandValidatorTests()
    {
        _sut = new ReopenTodoCommandValidator();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenIdIsNotEmpty()
    {
        // Arrange
        var command = new ReopenTodoCommand(Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        var command = new ReopenTodoCommand(Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(ReopenTodoCommand.Id));
    }
}
