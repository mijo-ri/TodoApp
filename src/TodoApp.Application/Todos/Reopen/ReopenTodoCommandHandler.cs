using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Todos.Reopen;

public class ReopenTodoCommandHandler
    : IRequestHandler<ReopenTodoCommand, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public ReopenTodoCommandHandler(
        ITodoRepository todoRepository,
        IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        ReopenTodoCommand request,
        CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (todo is null)
        {
            return TodoErrors.NotFound(request.Id);
        }

        todo.Reopen();
        await _todoRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TodoDto>(todo);
    }
}
