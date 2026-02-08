using ErrorOr;
using FluentAssertions;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Todos.Delete;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.Delete;

public class DeleteTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly DeleteTodoCommandHandler _sut;

    public DeleteTodoCommandHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);

        _sut = new DeleteTodoCommandHandler(_todoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveTodo_AndReturnDeleted_WhenTodoExists()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand(todoId);
        var todoItem = new TodoItem("Test");

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoItem);

        _todoRepositoryMock
            .Setup(r => r.Remove(todoItem));

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(Result.Deleted);

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.Remove(todoItem), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToSaveChangesAsync()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand(todoId);
        var todoItem = new TodoItem("Test");
        var token = new CancellationTokenSource().Token;

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todoId, token))
            .ReturnsAsync(todoItem);

        _todoRepositoryMock
            .Setup(r => r.Remove(todoItem));

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(token))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenTodoDoesNotExist()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand(todoId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == TodoErrors.NotFound(todoId));

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.Remove(It.IsAny<TodoItem>()), Times.Never);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
