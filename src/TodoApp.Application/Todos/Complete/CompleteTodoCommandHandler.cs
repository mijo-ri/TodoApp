using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Time;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Todos.Complete;

public class CompleteTodoCommandHandler
    : IRequestHandler<CompleteTodoCommand, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;
    private readonly IClock _clock;
    private readonly ICurrentUserService _currentUserService;

    public CompleteTodoCommandHandler(
        ITodoRepository todoRepository,
        IMapper mapper,
        IClock clock,
        ICurrentUserService currentUserService)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
        _clock = clock;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        CompleteTodoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return UserErrors.Unauthorized();
        }

        var todo = await _todoRepository.GetByIdForOwnerAsync(request.Id, userId.Value, cancellationToken);
        if (todo is null)
        {
            return TodoErrors.NotFound(request.Id);
        }

        todo.Complete(_clock.UtcNow);
        await _todoRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TodoDto>(todo);
    }
}
