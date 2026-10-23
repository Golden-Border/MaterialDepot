namespace MaterialDepot.Core.Settings;

/// <summary>
/// Keeps the settings between runs.
/// </summary>
public interface ISettingsStore
{
    /// <summary>
    /// The saved settings, or the defaults when nothing was saved yet.
    /// </summary>
    DepotSettings Load();

    void Save(DepotSettings settings);
}
