using FluentAssertions;
using MapsterMapper;
using Moq;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.Create;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Tests.Todos.Create;

public class CreateTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateTodoCommandHandler _sut;

    public CreateTodoCommandHandlerTests()
    {
        _todoRepositoryMock = new Mock<ITodoRepository>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
        _currentUserServiceMock = new Mock<ICurrentUserService>(MockBehavior.Strict);

        var userId = Guid.NewGuid();
        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(userId);

        _todoRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _sut = new CreateTodoCommandHandler(
            _todoRepositoryMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPersistTodo_AndReturnMappedDto()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedDto = CreateDtoFrom(command);

        _todoRepositoryMock
            .Setup(r => r.Add(It.IsAny<TodoItem>()));

        _mapperMock
            .Setup(m => m.Map<TodoDto>(It.IsAny<TodoItem>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(expectedDto);

        VerifyPersistCalled(timesAdd: Times.Once(), timesSave: Times.Once());
        VerifyMappingCalled(Times.Once());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToSaveChangesAsync()
    {
        // Arrange
        var command = CreateValidCommand();
        var token = new CancellationTokenSource().Token;

        _todoRepositoryMock
            .Setup(r => r.Add(It.IsAny<TodoItem>()));

        _mapperMock
            .Setup(m => m.Map<TodoDto>(It.IsAny<TodoItem>()))
            .Returns(CreateDummyDto());

        // Act
        await _sut.Handle(command, token);

        // Assert
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_AndNotPersist_WhenDomainExceptionOccurs()
    {
        // Arrange
        var command = new CreateTodoCommand(Title: "", Notes: null, DueDate: null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();

        VerifyPersistCalled(timesAdd: Times.Never(), timesSave: Times.Never());
        VerifyMappingCalled(Times.Never());
    }

    private void VerifyPersistCalled(Times timesAdd, Times timesSave)
    {
        _todoRepositoryMock.Verify(r => r.Add(It.IsAny<TodoItem>()), timesAdd);
        _todoRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), timesSave);
    }

    private void VerifyMappingCalled(Times times)
    {
        _mapperMock.Verify(m => m.Map<TodoDto>(It.IsAny<TodoItem>()), times);
    }

    private static CreateTodoCommand CreateValidCommand()
    {
        return new CreateTodoCommand(
            Title: "Buy milk",
            Notes: "Remember low fat",
            DueDate: new DateOnly(2026, 2, 10));
    }

    private static TodoDto CreateDtoFrom(CreateTodoCommand command)
    {
        return new TodoDto(
            Id: Guid.NewGuid(),
            Title: command.Title,
            Notes: command.Notes,
            DueDate: command.DueDate,
            IsCompleted: false,
            CompletedAt: null);
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
