using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularAgenda.Core.Repositories;
using ModularAgenda.Core.Services;
using ModularAgenda.Data;
using ModularAgenda.Data.Repositories;

namespace ModularAgenda.App;

public partial class App : Application
{
    private readonly ServiceProvider serviceProvider;
    private IServiceScope? windowScope;

    public IServiceProvider ServiceProvider => serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        windowScope = serviceProvider.CreateScope();
        var repository = windowScope.ServiceProvider.GetRequiredService<INoteRepository>();
        var noteService = new NoteService(repository);
        var viewModel = new MonthViewModel(noteService);
        var window = new MainWindow(viewModel);

        MainWindow = window;
        window.Show();
        await viewModel.InitializeAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        windowScope?.Dispose();
        serviceProvider.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(
            options => options.UseSqlite("Data Source=agenda.db"));

        services.AddScoped<INoteRepository, EfNoteRepository>();
        services.AddScoped<ITodoRepository, EfTodoRepository>();
        services.AddScoped<ICalendarEventRepository, EfCalendarEventRepository>();
        services.AddScoped<IReminderRepository, EfReminderRepository>();
    }
}
