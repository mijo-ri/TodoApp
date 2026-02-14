using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Security;

namespace TodoApp.Application.Todos.List;

public sealed class ListTodosQueryHandler
    : IRequestHandler<ListTodosQuery, List<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ListTodosQueryHandler(
        ITodoRepository todoRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<List<TodoDto>> Handle(
        ListTodosQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return new List<TodoDto>();
        }
        
        var userId = _currentUserService.UserId.Value;
        var todos = await _todoRepository.ListAsync(
            userId,
            request.IsCompleted,
            cancellationToken);

        return _mapper.Map<List<TodoDto>>(todos);
    }
}
