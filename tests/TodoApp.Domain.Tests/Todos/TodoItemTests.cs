using FluentAssertions;
using TodoApp.Domain.Todos;

namespace TodoApp.Domain.Tests.Todos;

public class TodoItemTests
{
    [Fact]
    public void Constructor_ShouldSetProperties_WhenValidArguments()
    {
        // Arrange
        var title = "Buy milk";
        var notes = "Remember to buy low fat";
        var dueDate = new DateOnly(2026, 2, 10);

        // Act
        var item = new TodoItem(title, notes, dueDate);

        // Assert
        item.Title.Value.Should().Be(title);
        item.Notes.Should().Be(notes);
        item.DueDate.Should().Be(dueDate);
        item.IsCompleted.Should().BeFalse();
        item.CompletedAt.Should().BeNull();
        item.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldTrimNotes_AndSetNull_WhenWhitespaceOrNull()
    {
        // Act
        var item1 = new TodoItem("Title", "   ");
        var item2 = new TodoItem("Title", null);

        // Assert
        item1.Notes.Should().BeNull();
        item2.Notes.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowDomainException_WhenNotesExceedMaxLength()
    {
        // Arrange
        var notes = new string('a', 2001);

        // Act
        var act = () => new TodoItem("Title", notes);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Notes must not exceed 2000 characters.");
    }

    [Fact]
    public void Update_ShouldChangeTitleNotesAndDueDate_WhenNotCompleted()
    {
        // Arrange
        var item = new TodoItem("Old", "Old notes", new DateOnly(2026, 2, 8));

        // Act
        item.Update("New", "New notes", new DateOnly(2026, 2, 9));

        // Assert
        item.Title.Value.Should().Be("New");
        item.Notes.Should().Be("New notes");
        item.DueDate.Should().Be(new DateOnly(2026, 2, 9));
    }

    [Fact]
    public void Update_ShouldThrowDomainException_WhenCompleted()
    {
        // Arrange
        var item = new TodoItem("Title");
        item.Complete(DateTimeOffset.UtcNow);

        // Act
        var act = () => item.Update("New", "Notes", null);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Completed todos cannot be edited.");
    }

    [Fact]
    public void Update_ShouldThrowDomainException_WhenNotesExceedMaxLength()
    {
        // Arrange
        var item = new TodoItem("Title");
        var notes = new string('a', 2001);

        // Act
        var act = () => item.Update("Title", notes, null);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Notes must not exceed 2000 characters.");
    }

    [Fact]
    public void Complete_ShouldSetIsCompletedAndCompletedAt_WhenNotAlreadyCompleted()
    {
        // Arrange
        var item = new TodoItem("Title");
        var now = DateTimeOffset.UtcNow;

        // Act
        item.Complete(now);

        // Assert
        item.IsCompleted.Should().BeTrue();
        item.CompletedAt.Should().Be(now);
    }

    [Fact]
    public void Complete_ShouldDoNothing_WhenAlreadyCompleted()
    {
        // Arrange
        var item = new TodoItem("Title");
        var first = DateTimeOffset.UtcNow;
        item.Complete(first);

        // Act
        var second = first.AddMinutes(5);
        item.Complete(second);

        // Assert
        item.IsCompleted.Should().BeTrue();
        item.CompletedAt.Should().Be(first);
    }

    [Fact]
    public void Reopen_ShouldSetIsCompletedFalseAndClearCompletedAt_WhenCompleted()
    {
        // Arrange
        var item = new TodoItem("Title");
        var now = DateTimeOffset.UtcNow;
        item.Complete(now);

        // Act
        item.Reopen();

        // Assert
        item.IsCompleted.Should().BeFalse();
        item.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Reopen_ShouldDoNothing_WhenNotCompleted()
    {
        // Arrange
        var item = new TodoItem("Title");

        // Act
        item.Reopen();

        // Assert
        item.IsCompleted.Should().BeFalse();
        item.CompletedAt.Should().BeNull();
    }
}
