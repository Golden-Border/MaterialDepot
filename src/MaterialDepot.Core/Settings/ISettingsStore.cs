namespace MaterialDepot.Core.Settings;

/// <summary>
/// Keeps the settings between runs.
/// </summary>
public interface ISettingsStore
{
    /// <summary>
    /// <see langword="true"/> when settings were saved before, <see langword="false"/> on the very first start.
    /// </summary>
    bool HasSavedSettings { get; }

    /// <summary>
    /// The saved settings, or the defaults when nothing was saved yet.
    /// </summary>
    DepotSettings Load();

    void Save(DepotSettings settings);
}
