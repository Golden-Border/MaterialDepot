using MaterialDepot.Core.Library;
using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.Services;

/// <summary>
/// What every tab works on: the settings and the last scan of the library.
/// </summary>
public sealed class DepotSession(ISettingsStore settingsStore, ITextureLibraryScanner libraryScanner)
{
    public DepotSettings Settings { get; private set; } = settingsStore.Load();

    public IReadOnlyList<TextureSet> Library { get; private set; } = [];

    public event EventHandler? SettingsChanged;

    public event EventHandler? LibraryChanged;

    public void SaveSettings(DepotSettings settings)
    {
        settingsStore.Save(settings);
        Settings = settings;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task RescanLibraryAsync()
    {
        var root = Settings.LibraryRoot;
        Library = await Task.Run(() => libraryScanner.Scan(root));
        LibraryChanged?.Invoke(this, EventArgs.Empty);
    }
}
