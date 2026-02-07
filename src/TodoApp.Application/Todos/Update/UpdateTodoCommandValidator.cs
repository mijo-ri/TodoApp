using FluentValidation;

namespace TodoApp.Application.Todos.Update;

public sealed class UpdateTodoCommandValidator
    : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
