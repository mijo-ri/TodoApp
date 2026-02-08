using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.Reopen;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.Reopen;

public class ReopenTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ReopenTodoCommandHandler _sut;

    public ReopenTodoCommandHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);

        _sut = new ReopenTodoCommandHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReopenTodo_AndReturnMappedDto()
    {
        // Arrange
        var todo = CreateCompletedTodoItem();
        var command = new ReopenTodoCommand(todo.Id);
        var expectedDto = CreateDtoFrom(todo);

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

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

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(todo.Id, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<TodoDto>(todo), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenTodoDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ReopenTodoCommand(id);

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<TodoDto>(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepositoryMethods()
    {
        // Arrange
        var todo = CreateCompletedTodoItem();
        var command = new ReopenTodoCommand(todo.Id);
        var token = new CancellationTokenSource().Token;

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todo.Id, token))
            .ReturnsAsync(todo);

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(token))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<TodoDto>(todo))
            .Returns(CreateDtoFrom(todo));

        // Act
        await _sut.Handle(command, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.GetByIdAsync(todo.Id, token), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(token), Times.Once);
    }

    private static TodoItem CreateCompletedTodoItem()
    {
        var item = new TodoItem("Completed Task", "Notes", new DateOnly(2026, 2, 10));
        item.Complete(DateTimeOffset.UtcNow);
        return item;
    }

    private static TodoDto CreateDtoFrom(TodoItem item)
    {
        return new TodoDto(
            Id: item.Id,
            Title: item.Title.ToString(),
            Notes: item.Notes,
            DueDate: item.DueDate,
            IsCompleted: item.IsCompleted,
            CompletedAt: item.CompletedAt);
    }
}
