using FluentAssertions;
using TodoApp.Application.Todos.Create;

namespace TodoApp.Application.Tests.Todos.Create;

public class CreateTodoCommandValidatorTests
{
    private readonly CreateTodoCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldSucceed_ForValidCommand()
    {
        // Arrange
        var command = new CreateTodoCommand(
            Title: "Buy groceries",
            Notes: "Milk, eggs, bread",
            DueDate: new DateOnly(2026, 2, 10));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateTodoCommand(
            Title: "",
            Notes: "Some notes",
            DueDate: null);

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
        var command = new CreateTodoCommand(
            Title: new string('a', 201),
            Notes: null,
            DueDate: null);

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
        var command = new CreateTodoCommand(
            Title: "Valid Title",
            Notes: new string('b', 2001),
            DueDate: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenNotesIsNull()
    {
        // Arrange
        var command = new CreateTodoCommand(
            Title: "Valid Title",
            Notes: null,
            DueDate: null);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
