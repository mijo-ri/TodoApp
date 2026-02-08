using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.List;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.List;

public class ListTodosQueryHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ListTodosQueryHandler _sut;

    public ListTodosQueryHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);

        _sut = new ListTodosQueryHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos_WhenTodosExist()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            CreateTodoItem("Task 1"),
            CreateTodoItem("Task 2")
        };
        var expectedDtos = todos.Select(CreateDtoFrom).ToList();
        var query = new ListTodosQuery(IsCompleted: null);

        _todoRepositoryMock
            .Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        _mapperMock
            .Setup(m => m.Map<List<TodoDto>>(todos))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedDtos);

        _todoRepositoryMock.Verify(r => r.ListAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<TodoDto>>(todos), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        // Arrange
        var todos = new List<TodoItem>();
        var expectedDtos = new List<TodoDto>();
        var query = new ListTodosQuery(IsCompleted: true);

        _todoRepositoryMock
            .Setup(r => r.ListAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        _mapperMock
            .Setup(m => m.Map<List<TodoDto>>(todos))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        _todoRepositoryMock.Verify(r => r.ListAsync(true, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<List<TodoDto>>(todos), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToListAsync()
    {
        // Arrange
        var todos = new List<TodoItem> { CreateTodoItem("Task") };
        var expectedDtos = todos.Select(CreateDtoFrom).ToList();
        var query = new ListTodosQuery(IsCompleted: false);
        var token = new CancellationTokenSource().Token;

        _todoRepositoryMock
            .Setup(r => r.ListAsync(false, token))
            .ReturnsAsync(todos);

        _mapperMock
            .Setup(m => m.Map<List<TodoDto>>(todos))
            .Returns(expectedDtos);

        // Act
        await _sut.Handle(query, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.ListAsync(false, token), Times.Once);
    }

    private static TodoItem CreateTodoItem(string title)
    {
        return new TodoItem(title, "Notes", new DateOnly(2026, 2, 10));
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
