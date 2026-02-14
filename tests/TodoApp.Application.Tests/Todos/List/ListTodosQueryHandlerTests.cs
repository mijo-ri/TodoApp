using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.List;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.List;

public class ListTodosQueryHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ListTodosQueryHandler _sut;

    public ListTodosQueryHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
        _currentUserServiceMock = new Mock<ICurrentUserService>(MockBehavior.Strict);

        _sut = new ListTodosQueryHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos_WhenTodosExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var todos = new List<TodoItem>
        {
            CreateTodoItem("Task 1"),
            CreateTodoItem("Task 2")
        };
        var expectedDtos = todos.Select(CreateDtoFrom).ToList();
        var query = new ListTodosQuery(IsCompleted: null);

        _todoRepositoryMock
            .Setup(r => r.ListAsync(userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        _mapperMock
            .Setup(m => m.Map<List<TodoDto>>(todos))
            .Returns(expectedDtos);
        
        _currentUserServiceMock
            .Setup(c => c.UserId)
            .Returns(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedDtos);

        _todoRepositoryMock.Verify(r => r.ListAsync(userId, null, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<TodoDto>>(todos), Times.Once);
        _currentUserServiceMock.VerifyGet(s => s.UserId, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var todos = new List<TodoItem>();
        var expectedDtos = new List<TodoDto>();
        var query = new ListTodosQuery(IsCompleted: true);

        _todoRepositoryMock
            .Setup(r => r.ListAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        _mapperMock
            .Setup(m => m.Map<List<TodoDto>>(todos))
            .Returns(expectedDtos);
        
        _currentUserServiceMock
            .Setup(c => c.UserId)
            .Returns(userId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        _todoRepositoryMock.Verify(r => r.ListAsync(userId, true, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<TodoDto>>(todos), Times.Once);
        _currentUserServiceMock.VerifyGet(s => s.UserId, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToListAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var todos = new List<TodoItem> { CreateTodoItem("Task") };
        var expectedDtos = todos.Select(CreateDtoFrom).ToList();
        var query = new ListTodosQuery(IsCompleted: false);
        var token = new CancellationTokenSource().Token;

        _todoRepositoryMock
            .Setup(r => r.ListAsync(userId, false, token))
            .ReturnsAsync(todos);

        _mapperMock
            .Setup(m => m.Map<List<TodoDto>>(todos))
            .Returns(expectedDtos);
        
        _currentUserServiceMock
            .Setup(c => c.UserId)
            .Returns(userId);

        // Act
        await _sut.Handle(query, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.ListAsync(userId, false, token), Times.Once);
        _currentUserServiceMock.VerifyGet(s => s.UserId, Times.AtLeastOnce);
    }

    private static TodoItem CreateTodoItem(string title)
    {
        var userId = Guid.NewGuid();
        return new TodoItem(userId, title, "Notes", new DateOnly(2026, 2, 10));
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
