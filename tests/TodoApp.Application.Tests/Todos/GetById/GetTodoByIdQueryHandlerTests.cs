using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.GetById;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.GetById;

public class GetTodoByIdQueryHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetTodoByIdQueryHandler _sut;

    public GetTodoByIdQueryHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
        _currentUserServiceMock = new Mock<ICurrentUserService>(MockBehavior.Strict);

        _sut = new GetTodoByIdQueryHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDto_WhenTodoExists()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetTodoByIdQuery(todoId);
        var todoItem = CreateTodoItem(todoId);
        var expectedDto = CreateDtoFrom(todoItem);

        _currentUserServiceMock.SetupGet(s => s.UserId).Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoItem);

        _mapperMock
            .Setup(m => m.Map<TodoDto>(todoItem))
            .Returns(expectedDto);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedDto);

        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<TodoDto>(todoItem), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToGetByIdForOwnerAsync()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetTodoByIdQuery(todoId);
        var todoItem = CreateTodoItem(todoId);
        var token = new CancellationTokenSource().Token;

        _currentUserServiceMock.SetupGet(s => s.UserId).Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(todoId, userId, token))
            .ReturnsAsync(todoItem);

        _mapperMock
            .Setup(m => m.Map<TodoDto>(todoItem))
            .Returns(CreateDummyDto());

        // Act
        await _sut.Handle(query, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(todoId, userId, token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenTodoDoesNotExist()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetTodoByIdQuery(todoId);

        _currentUserServiceMock.SetupGet(s => s.UserId).Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == TodoErrors.NotFound(todoId));

        _todoRepositoryMock.Verify(r => r.GetByIdForOwnerAsync(todoId, userId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<TodoDto>(It.IsAny<TodoItem>()), Times.Never);
    }

    private static TodoItem CreateTodoItem(Guid id)
    {
        var userId = Guid.NewGuid();
        var item = new TodoItem(userId, "Test title", "Test notes", new DateOnly(2026, 2, 10));
        typeof(TodoItem).GetProperty("Id")!.SetValue(item, id);
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

    private static TodoDto CreateDummyDto()
    {
        return new TodoDto(
            Id: Guid.NewGuid(),
            Title: "Title",
            Notes: null,
            DueDate: null,
            IsCompleted: false,
            CompletedAt: null);
    }
}
