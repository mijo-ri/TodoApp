using FluentValidation;

namespace TodoApp.Application.Todos.Create;

public sealed class CreateTodoCommandValidator
    : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
