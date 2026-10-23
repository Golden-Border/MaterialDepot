using System.Windows;
using MaterialDepot.App.ViewModels;

namespace MaterialDepot.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Coming back from the browser is when a new download is most likely there.
    /// </summary>
    private void OnActivated(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel { Inbox.RescanCommand: var rescan } && rescan.CanExecute(null))
        {
            rescan.Execute(null);
        }
    }
}
