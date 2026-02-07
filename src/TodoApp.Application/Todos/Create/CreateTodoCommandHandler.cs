using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Common.Errors;
using TodoApp.Domain;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Todos.Create;

public sealed class CreateTodoCommandHandler
    : IRequestHandler<CreateTodoCommand, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public CreateTodoCommandHandler(
        ITodoRepository todoRepository,
        IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        CreateTodoCommand request,
        CancellationToken cancellationToken)
    {
        TodoItem todo;

        try
        {
            todo = new TodoItem(
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
