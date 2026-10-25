using System.ComponentModel;
using System.Windows;
using MaterialDepot.App.ViewModels;

namespace MaterialDepot.App.Views;

public partial class SetupWindow : Window
{
    public SetupWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is INotifyPropertyChanged old)
            {
                old.PropertyChanged -= OnViewModelChanged;
            }

            if (e.NewValue is INotifyPropertyChanged current)
            {
                current.PropertyChanged += OnViewModelChanged;
            }
        };
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SetupViewModel.DialogResult) && sender is SetupViewModel { DialogResult: { } result })
        {
            DialogResult = result;
        }
    }
}
