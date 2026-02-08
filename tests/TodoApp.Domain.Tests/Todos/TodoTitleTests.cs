using FluentAssertions;
using TodoApp.Domain.Todos;

namespace TodoApp.Domain.Tests.Todos;

public class TodoTitleTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Create_ShouldThrowDomainException_WhenNullOrWhitespace(string? input)
    {
        // Act
        var act = () => TodoTitle.Create(input);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage("Title must not be empty.");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenInputHasLeadingOrTrailingWhitespace()
    {
        // Act
        var title = TodoTitle.Create("  Buy milk  ");

        // Assert
        title.Value.Should().Be("Buy milk");
    }

    [Fact]
    public void Create_ShouldReturnTodoTitle_WithSameValue_WhenValid()
    {
        // Arrange
        const string input = "Buy milk";

        // Act
        var title = TodoTitle.Create(input);

        // Assert
        title.Value.Should().Be(input);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenExceedsMaxLength()
    {
        // Arrange
        var input = new string('a', TodoTitle.MaxLength + 1);

        // Act
        var act = () => TodoTitle.Create(input);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage($"Title must be at most {TodoTitle.MaxLength} characters.");
    }

    [Fact]
    public void Create_ShouldAllowExactlyMaxLength()
    {
        // Arrange
        var input = new string('a', TodoTitle.MaxLength);

        // Act
        var title = TodoTitle.Create(input);

        // Assert
        title.Value.Should().Be(input);
        title.Value.Length.Should().Be(TodoTitle.MaxLength);
    }

    [Fact]
    public void RecordEquality_ShouldBeValueBased()
    {
        // Arrange
        var a = TodoTitle.Create("Buy milk");
        var b = TodoTitle.Create("Buy milk");

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var title = TodoTitle.Create("Buy milk");

        // Act
        var str = title.ToString();

        // Assert
        str.Should().Be("Buy milk");
    }
}
