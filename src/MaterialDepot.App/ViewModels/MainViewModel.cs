using MaterialDepot.App.Services;
using MaterialDepot.Core.Setup;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The main window: the three tabs.
/// </summary>
public sealed class MainViewModel(
    DepotSession session,
    SetupGuide setupGuide,
    IDialogService dialogs,
    LibraryViewModel library,
    InboxViewModel inbox,
    SettingsViewModel settings)
{
    public LibraryViewModel Library { get; } = library;

    public InboxViewModel Inbox { get; } = inbox;

    public SettingsViewModel Settings { get; } = settings;

    /// <summary>
    /// The setup guide on the first start, then the first scan; the inbox follows the library on its own.
    /// </summary>
    public Task StartAsync()
    {
        if (setupGuide.IsFirstRun && !dialogs.ShowSetup(new SetupViewModel(session, setupGuide, dialogs)))
        {
            // Skipped: keep the defaults for good, so the guide does not come back on every start.
            session.SaveSettings(session.Settings);
        }

        return session.RescanLibraryAsync();
    }
}
