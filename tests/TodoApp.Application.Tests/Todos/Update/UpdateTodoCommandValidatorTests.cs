using FluentAssertions;
using TodoApp.Application.Todos.Update;

namespace TodoApp.Application.Tests.Todos.Update;

public class UpdateTodoCommandValidatorTests
{
    private readonly UpdateTodoCommandValidator _sut;

    public UpdateTodoCommandValidatorTests()
    {
        _sut = new UpdateTodoCommandValidator();
    }

    [Fact]
    public void Validate_ShouldSucceed_ForValidCommand()
    {
        // Arrange
        var command = new UpdateTodoCommand(
            Id: Guid.NewGuid(),
            Title: "Update title",
            Notes: "Some notes",
            DueDate: new DateOnly(2026, 2, 10)
        );

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateTodoCommand(
            Id: Guid.Empty,
            Title: "Valid title",
            Notes: "Notes",
            DueDate: null
        );

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new UpdateTodoCommand(
            Id: Guid.NewGuid(),
            Title: "",
            Notes: "Notes",
            DueDate: null
        );

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateTodoCommand(
            Id: Guid.NewGuid(),
            Title: new string('a', 201),
            Notes: "Notes",
            DueDate: null
        );

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenNotesExceedMaxLength()
    {
        // Arrange
        var command = new UpdateTodoCommand(
            Id: Guid.NewGuid(),
            Title: "Valid title",
            Notes: new string('b', 2001),
            DueDate: null
        );

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes");
    }
}
