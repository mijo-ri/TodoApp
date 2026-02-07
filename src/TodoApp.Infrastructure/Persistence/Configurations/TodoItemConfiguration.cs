using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Todos;

namespace TodoApp.Infrastructure.Persistence.Configurations;

public sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("Todos");

        builder.HasKey(x => x.Id);

        // ValueConverter: TodoTitle <-> string
        builder.Property(x => x.Title)
            .HasConversion(
                v => v.Value,
                v => TodoTitle.Create(v))
            .HasColumnName("Title")
            .HasMaxLength(TodoTitle.MaxLength)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.DueDate);

        builder.Property(x => x.IsCompleted)
            .IsRequired();

        builder.Property(x => x.CompletedAt);
    }
}
