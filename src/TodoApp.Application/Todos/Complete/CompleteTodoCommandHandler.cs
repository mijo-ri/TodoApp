using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Time;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Todos.Complete;

public class CompleteTodoCommandHandler
    : IRequestHandler<CompleteTodoCommand, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public CompleteTodoCommandHandler(
        ITodoRepository todoRepository,
        IMapper mapper,
        IClock clock)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
        _clock = clock;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        CompleteTodoCommand request,
        CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (todo is null)
        {
            return TodoErrors.NotFound(request.Id);
        }

        todo.Complete(_clock.UtcNow);
        await _todoRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TodoDto>(todo);
    }
}
