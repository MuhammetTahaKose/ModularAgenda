using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularAgenda.Core.Domain;
using ModularAgenda.Core.Repositories;
using ModularAgenda.Core.Services;
using ModularAgenda.Core.Services.Calendar;
using ModularAgenda.Data;
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
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        string dataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ModularAgenda");
        Directory.CreateDirectory(dataFolder); // klasör zaten varsa bir şey yapmaz
        string databasePath = Path.Combine(dataFolder, "agenda.db");

        // Uygulama tek kullanıcılı ve tek pencereli; bu yüzden context, repository'ler ve servisler Singleton.
        services.AddDbContext<AppDbContext>(
            options => options.UseSqlite($"Data Source={databasePath}"),
            ServiceLifetime.Singleton);

        // Repository'ler
        services.AddSingleton<INoteRepository, EfNoteRepository>();
        services.AddSingleton<ITodoRepository, EfTodoRepository>();
        services.AddSingleton<ICalendarEventRepository, EfCalendarEventRepository>();
        services.AddSingleton<IReminderRepository, EfReminderRepository>();
        services.AddSingleton<IRepository<Habit>, EfRepository<Habit>>();

        // Servisler
        services.AddSingleton<LinkedItemsCleaner>();
        services.AddSingleton<NoteService>();
        services.AddSingleton<CalendarService>();
        services.AddSingleton<TodoService>();
        services.AddSingleton<EventService>();
        services.AddSingleton<HabitService>();

        // UI ekibi: ViewModel'leri buraya kaydedin

        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
