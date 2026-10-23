using MaterialDepot.App.Services;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The main window: the three tabs.
/// </summary>
public sealed class MainViewModel(DepotSession session, LibraryViewModel library, InboxViewModel inbox, SettingsViewModel settings)
{
    public LibraryViewModel Library { get; } = library;

    public InboxViewModel Inbox { get; } = inbox;

    public SettingsViewModel Settings { get; } = settings;

    /// <summary>
    /// The first scan; the inbox follows the library on its own.
    /// </summary>
    public Task StartAsync() => session.RescanLibraryAsync();
}
