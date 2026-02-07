using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;

namespace TodoApp.Application.Todos.List;

public sealed class ListTodosQueryHandler
    : IRequestHandler<ListTodosQuery, List<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public ListTodosQueryHandler(
        ITodoRepository todoRepository,
        IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<List<TodoDto>> Handle(
        ListTodosQuery request,
        CancellationToken cancellationToken)
    {
        var todos = await _todoRepository.ListAsync(
            request.IsCompleted,
            cancellationToken);

        return _mapper.Map<List<TodoDto>>(todos);
    }
}
