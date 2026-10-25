using MaterialDepot.App.ViewModels;

namespace MaterialDepot.App.Services;

/// <summary>
/// The windows and message boxes a view model opens.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows the import dialog; <see langword="true"/> when an import ran.
    /// </summary>
    bool ShowImport(ImportViewModel import);

    /// <summary>
    /// Shows the setup guide; <see langword="true"/> when it saved the settings, <see langword="false"/> when it was skipped or closed.
    /// </summary>
    bool ShowSetup(SetupViewModel setup);

    bool Confirm(string message);

    void ShowError(string message);

    void OpenFolder(string path);

    string? PickFolder(string? initialDirectory);
}
