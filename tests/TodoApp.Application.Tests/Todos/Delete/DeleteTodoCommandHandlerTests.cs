using ErrorOr;
using FluentAssertions;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Todos.Delete;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.Delete;

public class DeleteTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteTodoCommandHandler _sut;

    public DeleteTodoCommandHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _currentUserServiceMock = new Mock<ICurrentUserService>(MockBehavior.Strict);

        _sut = new DeleteTodoCommandHandler(_todoRepositoryMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveTodo_AndReturnDeleted_WhenTodoExists()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand(todoId);
        var userId = Guid.NewGuid();
        var todoItem = new TodoItem(userId, "Test");

        _currentUserServiceMock
            .SetupGet(c => c.UserId)
            .Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()))
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

        _currentUserServiceMock.VerifyGet(c => c.UserId, Times.Once);
        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.Remove(todoItem), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToSaveChangesAsync()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand(todoId);
        var userId = Guid.NewGuid();
        var todoItem = new TodoItem(userId, "Test");
        var token = new CancellationTokenSource().Token;

        _currentUserServiceMock
            .SetupGet(c => c.UserId)
            .Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(todoId, userId, token))
            .ReturnsAsync(todoItem);

        _todoRepositoryMock
            .Setup(r => r.Remove(todoItem));

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(token))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, token);

        // Assert
        _currentUserServiceMock.VerifyGet(c => c.UserId, Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenTodoDoesNotExist()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand(todoId);
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(c => c.UserId)
            .Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == TodoErrors.NotFound(todoId));

        _currentUserServiceMock.VerifyGet(c => c.UserId, Times.Once);
        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.Remove(It.IsAny<TodoItem>()), Times.Never);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
