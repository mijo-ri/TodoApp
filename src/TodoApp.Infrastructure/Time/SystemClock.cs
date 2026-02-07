using TodoApp.Application.Abstractions.Time;

namespace TodoApp.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTimeOffset UtcNow
    {
        get
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
