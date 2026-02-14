using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Todos.GetById;

public sealed class GetTodoByIdQueryHandler
    : IRequestHandler<GetTodoByIdQuery, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetTodoByIdQueryHandler(
        ITodoRepository todoRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        GetTodoByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return UserErrors.Unauthorized();
        }
        
        var todo = await _todoRepository.GetByIdForOwnerAsync(
            request.Id,
            userId.Value,
            cancellationToken);

        if (todo is null)
            return TodoErrors.NotFound(request.Id);

        return _mapper.Map<TodoDto>(todo);
    }
}
