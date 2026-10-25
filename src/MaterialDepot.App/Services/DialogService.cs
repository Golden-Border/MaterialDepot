using System.Diagnostics;
using System.IO;
using System.Windows;
using MaterialDepot.App.ViewModels;
using MaterialDepot.App.Views;
using Microsoft.Win32;

namespace MaterialDepot.App.Services;

public sealed class DialogService : IDialogService
{
    private const string Caption = "Material Depot";

    public bool ShowImport(ImportViewModel import)
    {
        var window = new ImportWindow { DataContext = import, Owner = Application.Current.MainWindow };
        // Closed with the window frame after an import counts as imported too.
        return window.ShowDialog() == true || import.Result is not null;
    }

    public bool ShowSetup(SetupViewModel setup)
    {
        var window = new SetupWindow { DataContext = setup, Owner = Application.Current.MainWindow };
        return window.ShowDialog() == true;
    }

    public bool Confirm(string message) =>
        MessageBox.Show(Application.Current.MainWindow, message, Caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public void ShowError(string message) =>
        MessageBox.Show(Application.Current.MainWindow, message, Caption, MessageBoxButton.OK, MessageBoxImage.Error);

    public void OpenFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
        }
    }

    public string? PickFolder(string? initialDirectory)
    {
        var dialog = new OpenFolderDialog();
        if (initialDirectory is not null && Directory.Exists(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        return dialog.ShowDialog(Application.Current.MainWindow) == true ? dialog.FolderName : null;
    }
}
