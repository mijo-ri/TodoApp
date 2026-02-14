using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;
using TodoApp.Domain;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Todos.Create;

public sealed class CreateTodoCommandHandler
    : IRequestHandler<CreateTodoCommand, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateTodoCommandHandler(
        ITodoRepository todoRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        CreateTodoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return UserErrors.Unauthorized();
        }
        
        TodoItem todo;

        try
        {
            todo = new TodoItem(
                _currentUserService.UserId.Value,
                request.Title,
                request.Notes,
                request.DueDate);
        }
        catch (DomainException ex)
        {
            return TodoErrors.Domain(ex.Message);
        }

        _todoRepository.Add(todo);
        await _todoRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TodoDto>(todo);
    }
}
