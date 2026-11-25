using System.Windows;
using ModularAgenda.Core.Services;
using ModularAgenda.Data;

namespace ModularAgenda.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var repository = new InMemoryNoteRepository();
        var noteService = new NoteService(repository);
        var viewModel = new MonthViewModel(noteService);
        var window = new MainWindow(viewModel);
        MainWindow = window;
        window.Show();
        await viewModel.InitializeAsync();
    }
}

