using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Time;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.Complete;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.Complete;

public class CompleteTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CompleteTodoCommandHandler _sut;

    public CompleteTodoCommandHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
        _clockMock = new Mock<IClock>(MockBehavior.Strict);
        _currentUserServiceMock = new Mock<ICurrentUserService>(MockBehavior.Strict);

        _sut = new CompleteTodoCommandHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object,
            _clockMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var command = CreateValidCommand();
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .SetupGet(c => c.UserId)
            .Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(command.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();

        _currentUserServiceMock.VerifyGet(c => c.UserId, Times.Once);
        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(command.Id, userId, It.IsAny<CancellationToken>()), Times.Once);
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
        var userId = Guid.NewGuid();
        var expectedDto = CreateDtoFrom(todo, now);

        _currentUserServiceMock
            .SetupGet(c => c.UserId)
            .Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(command.Id, userId, It.IsAny<CancellationToken>()))
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

        _currentUserServiceMock.VerifyGet(c => c.UserId, Times.Once);
        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(command.Id, userId, It.IsAny<CancellationToken>()), Times.Once);
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
        var userId = Guid.NewGuid();
        var token = new CancellationTokenSource().Token;

        _currentUserServiceMock
            .SetupGet(c => c.UserId)
            .Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(command.Id, userId, token))
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
        _currentUserServiceMock.VerifyGet(c => c.UserId, Times.Once);
        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(command.Id, userId, token), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(token), Times.Once);
    }

    private static CompleteTodoCommand CreateValidCommand()
    {
        return new CompleteTodoCommand(Guid.NewGuid());
    }

    private static TodoItem CreateTodoItem(Guid id, string title)
    {
        var userId = Guid.NewGuid();
        var todo = new TodoItem(userId, title);
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