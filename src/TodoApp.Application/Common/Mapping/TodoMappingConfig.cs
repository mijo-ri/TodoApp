using Mapster;
using TodoApp.Application.Todos;
using TodoApp.Domain.Todos;

namespace TodoApp.Application.Common.Mapping;

public sealed class TodoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TodoItem, TodoDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Notes, src => src.Notes)
            .Map(dest => dest.DueDate, src => src.DueDate)
            .Map(dest => dest.IsCompleted, src => src.IsCompleted)
            .Map(dest => dest.CompletedAt, src => src.CompletedAt);
    }
}
