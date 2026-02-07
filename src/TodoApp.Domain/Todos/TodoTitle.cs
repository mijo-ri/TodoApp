namespace TodoApp.Domain.Todos;

public sealed record TodoTitle
{
    public const int MaxLength = 200;

    public string Value { get; private set; } = default!;

    // needed by EF Core (private parameterless ctor)
    private TodoTitle() { }

    private TodoTitle(string value)
    {
        Value = value;
    }

    public static TodoTitle Create(string? value)
    {
        value = (value ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Title must not be empty.");

        if (value.Length > MaxLength)
            throw new DomainException($"Title must be at most {MaxLength} characters.");

        return new TodoTitle(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
