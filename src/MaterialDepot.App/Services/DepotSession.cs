using MaterialDepot.Core.Library;
using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.Services;

/// <summary>
/// What every tab works on: the settings and the last scan of the library.
/// </summary>
public sealed class DepotSession(ISettingsStore settingsStore, ITextureLibraryScanner libraryScanner)
{
    private MapFileParser? parser;

    public DepotSettings Settings { get; private set; } = settingsStore.Load();

    /// <summary>
    /// Recognises map files by the file name patterns of the current settings.
    /// </summary>
    public MapFileParser Parser => parser ??= Settings.CreateParser();

    public IReadOnlyList<TextureSet> Library { get; private set; } = [];

    public event EventHandler? SettingsChanged;

    public event EventHandler? LibraryChanged;

    public void SaveSettings(DepotSettings settings)
    {
        settingsStore.Save(settings);
        Settings = settings;
        parser = null;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task RescanLibraryAsync()
    {
        var root = Settings.LibraryRoot;
        var libraryParser = Parser;
        Library = await Task.Run(() => libraryScanner.Scan(root, libraryParser));
        LibraryChanged?.Invoke(this, EventArgs.Empty);
    }
}
