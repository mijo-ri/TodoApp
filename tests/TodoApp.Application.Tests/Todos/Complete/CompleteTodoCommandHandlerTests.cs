using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Time;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.Complete;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.Complete;

public class CompleteTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IClock> _clockMock;
    private readonly CompleteTodoCommandHandler _sut;

    public CompleteTodoCommandHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
        _clockMock = new Mock<IClock>(MockBehavior.Strict);

        _sut = new CompleteTodoCommandHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object,
            _clockMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var command = CreateValidCommand();

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.VerifyNoOtherCalls();
        _mapperMock.VerifyNoOtherCalls();
        _clockMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldCompleteTodo_AndReturnMappedDto()
    {
        // Arrange
        var command = CreateValidCommand();
        var todo = CreateTodoItem(command.Id, "Test");
        var now = DateTimeOffset.UtcNow;
        var expectedDto = CreateDtoFrom(todo, now);

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        _clockMock
            .Setup(c => c.UtcNow)
            .Returns(now);

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<TodoDto>(todo))
            .Returns(expectedDto);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedDto);

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        _clockMock.Verify(c => c.UtcNow, Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<TodoDto>(todo), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepositoryMethods()
    {
        // Arrange
        var command = CreateValidCommand();
        var todo = CreateTodoItem(command.Id, "Test");
        var now = DateTimeOffset.UtcNow;
        var token = new CancellationTokenSource().Token;

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, token))
            .ReturnsAsync(todo);

        _clockMock
            .Setup(c => c.UtcNow)
            .Returns(now);

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(token))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<TodoDto>(todo))
            .Returns(CreateDtoFrom(todo, now));

        // Act
        await _sut.Handle(command, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.GetByIdAsync(command.Id, token), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(token), Times.Once);
    }

    private static CompleteTodoCommand CreateValidCommand()
    {
        return new CompleteTodoCommand(Guid.NewGuid());
    }

    private static TodoItem CreateTodoItem(Guid id, string title)
    {
        var todo = new TodoItem(title);
        typeof(TodoItem).GetProperty("Id")!.SetValue(todo, id);
        return todo;
    }

    private static TodoDto CreateDtoFrom(TodoItem todo, DateTimeOffset completedAt)
    {
        return new TodoDto(
            Id: todo.Id,
            Title: todo.Title.ToString(),
            Notes: todo.Notes,
            DueDate: todo.DueDate,
            IsCompleted: true,
            CompletedAt: completedAt
        );
    }
}