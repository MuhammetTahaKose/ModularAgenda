using System.Windows;
using Microsoft.EntityFrameworkCore;
using ModularAgenda.Data;
using Microsoft.Extensions.DependencyInjection;
using ModularAgenda.Core.Repositories;
using ModularAgenda.Data.Repositories;

namespace ModularAgenda.App;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Initialize Database
        using var context = ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=agenda.db"));
        services.AddScoped<INoteRepository, EfNoteRepository>();
        services.AddScoped<ITodoRepository, EfTodoRepository>();
        services.AddScoped<ICalendarEventRepository, EfCalendarEventRepository>();
        services.AddScoped<IReminderRepository, EfReminderRepository>();
        // Add other services...
    }
}

