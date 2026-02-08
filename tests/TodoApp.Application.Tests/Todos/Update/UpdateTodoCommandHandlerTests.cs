using ErrorOr;
using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Common.Errors;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.Update;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.Update;

public class UpdateTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateTodoCommandHandler _sut;

    public UpdateTodoCommandHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);

        _sut = new UpdateTodoCommandHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTodo_AndReturnMappedDto()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = CreateValidCommand(todoId);
        var todoItem = CreateTodoItem(todoId);
        var expectedDto = CreateDtoFrom(todoItem);

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoItem);

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<TodoDto>(todoItem))
            .Returns(expectedDto);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedDto);

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<TodoDto>(todoItem), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepositoryMethods()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = CreateValidCommand(todoId);
        var todoItem = CreateTodoItem(todoId);
        var token = new CancellationTokenSource().Token;

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todoId, token))
            .ReturnsAsync(todoItem);

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(token))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<TodoDto>(todoItem))
            .Returns(CreateDummyDto());

        // Act
        await _sut.Handle(command, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.GetByIdAsync(todoId, token), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenTodoDoesNotExist()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = CreateValidCommand(todoId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == TodoErrors.NotFound(todoId));

        _todoRepositoryMock.Verify(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()), Times.Once);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<TodoDto>(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnDomainError_AndNotPersist_WhenDomainExceptionOccurs()
    {
        // Arrange
        var todoId = Guid.NewGuid();

        var invalidCommand = new UpdateTodoCommand(
            Id: todoId,
            Title: "", // Invalid title that will cause a DomainException in the Update method
            Notes: "Whole grain",
            DueDate: new DateOnly(2026, 2, 11));

        var todoItem = CreateTodoItem(todoId);

        _todoRepositoryMock
            .Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoItem);

        // Act
        var result = await _sut.Handle(invalidCommand, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Type == ErrorType.Validation || e == TodoErrors.Domain(It.IsAny<string>()));
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<TodoDto>(It.IsAny<TodoItem>()), Times.Never);
    }

    private static UpdateTodoCommand CreateValidCommand(Guid id)
    {
        return new UpdateTodoCommand(
            Id: id,
            Title: "Buy bread",
            Notes: "Whole grain",
            DueDate: new DateOnly(2026, 2, 11));
    }

    private static TodoItem CreateTodoItem(Guid id)
    {
        var item = new TodoItem("Old title", "Old notes", new DateOnly(2026, 2, 10));
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
