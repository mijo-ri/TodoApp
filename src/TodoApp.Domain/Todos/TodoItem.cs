namespace TodoApp.Domain.Todos;

public sealed class TodoItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Title { get; private set; } = default!;
    public string? Notes { get; private set; }
    public DateOnly? DueDate { get; private set; }

    public bool IsCompleted { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // needed by EF Core (private parameterless ctor)
    private TodoItem() { }

    public TodoItem(string title, string? notes = null, DateOnly? dueDate = null)
    {
        SetTitle(title);
        SetNotes(notes);
        SetDueDate(dueDate);

        IsCompleted = false;
        CompletedAt = null;
    }

    public void Update(string title, string? notes, DateOnly? dueDate)
    {
        if (IsCompleted) throw new DomainException("Completed todos cannot be edited.");

        SetTitle(title);
        SetNotes(notes);
        SetDueDate(dueDate);
    }

    public void Complete(DateTimeOffset now)
    {
        if (IsCompleted) return;

        IsCompleted = true;
        CompletedAt = now;
    }

    public void Reopen()
    {
        if (!IsCompleted) return;

        IsCompleted = false;
        CompletedAt = null;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title must not be empty.");

        if (title.Trim().Length > 200)
            throw new DomainException("Title must not exceed 200 characters.");

        Title = title.Trim();
    }

    private void SetNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            Notes = null;
            return;
        }

        if (notes.Trim().Length > 2000)
            throw new DomainException("Notes must not exceed 2000 characters.");

        Notes = notes.Trim();
    }

    private void SetDueDate(DateOnly? dueDate)
    {
        DueDate = dueDate;
    }
}
