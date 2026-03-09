using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using TabsNotesThings.ViewModels;

namespace TabsNotesThings.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        this.DataContext = mainWindowViewModel;
    }
    
    public MainWindow()
    { 
        InitializeComponent();
    }
}