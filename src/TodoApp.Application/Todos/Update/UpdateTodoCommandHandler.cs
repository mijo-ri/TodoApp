using ErrorOr;
using MapsterMapper;
using MediatR;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Common.Errors;
using TodoApp.Domain;

namespace TodoApp.Application.Todos.Update;

public sealed class UpdateTodoCommandHandler
    : IRequestHandler<UpdateTodoCommand, ErrorOr<TodoDto>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IMapper _mapper;

    public UpdateTodoCommandHandler(
        ITodoRepository todoRepository,
        IMapper mapper)
    {
        _todoRepository = todoRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<TodoDto>> Handle(
        UpdateTodoCommand request,
        CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (todo is null)
        {
            return TodoErrors.NotFound(request.Id);
        }

        try
        {
            todo.Update(
                request.Title,
                request.Notes,
                request.DueDate);
        }
        catch (DomainException ex)
        {
            return TodoErrors.Domain(ex.Message);
        }

        await _todoRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TodoDto>(todo);
    }
}
