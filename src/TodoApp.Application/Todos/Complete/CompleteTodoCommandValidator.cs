using FluentValidation;

namespace TodoApp.Application.Todos.Complete;

public class CompleteTodoCommandValidator
    : AbstractValidator<CompleteTodoCommand>
{
    public CompleteTodoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
