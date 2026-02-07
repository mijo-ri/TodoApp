using FluentValidation;

namespace TodoApp.Application.Todos.Reopen;

public class ReopenTodoCommandValidator
    : AbstractValidator<ReopenTodoCommand>
{
    public ReopenTodoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
