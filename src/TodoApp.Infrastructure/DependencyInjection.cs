using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Abstractions.Persistence;
using TodoApp.Application.Abstractions.Time;
using TodoApp.Infrastructure.Persistence;
using TodoApp.Infrastructure.Persistence.Repositories;
using TodoApp.Infrastructure.Time;

namespace TodoApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TodoDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("TodoDb"),
                x => x.MigrationsAssembly("TodoApp.Infrastructure")));

        services.AddScoped<ITodoRepository, EfTodoRepository>();

        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
