using System.Windows;

namespace ModularAgenda.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MonthViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}