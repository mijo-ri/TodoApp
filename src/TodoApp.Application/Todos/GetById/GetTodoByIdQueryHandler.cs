using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Common.Errors;

namespace TodoApp.Application.Todos.GetById;

public sealed class GetTodoByIdQueryHandler
    : IRequestHandler<GetTodoByIdQuery, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public GetTodoByIdQueryHandler(
        ITodoRepository todoRepository,
        IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        GetTodoByIdQuery request,
        CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (todo is null)
            return TodoErrors.NotFound(request.Id);

        return _mapper.Map<TodoDto>(todo);
    }
}
