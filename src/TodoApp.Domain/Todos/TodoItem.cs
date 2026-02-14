namespace TodoApp.Domain.Todos;

public sealed class TodoItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }

    public TodoTitle Title { get; private set; } = default!;
    public string? Notes { get; private set; }
    public DateOnly? DueDate { get; private set; }

    public bool IsCompleted { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // needed by EF Core (private parameterless ctor)
    private TodoItem() { }

    public TodoItem(
        Guid userId,
        string title,
        string? notes = null,
        DateOnly? dueDate = null)
    {
        SetOwnerId(userId);
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

    private void SetOwnerId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("OwnerId must not be empty.");
        
        OwnerId = userId;
    }
    
    private void SetTitle(string title)
    {
        Title = TodoTitle.Create(title);
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
