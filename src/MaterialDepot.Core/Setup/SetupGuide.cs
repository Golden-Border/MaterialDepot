using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Stride;

namespace MaterialDepot.Core.Setup;

/// <summary>
/// The rules behind the first-run setup: when it is due and what it makes of the settings.
/// </summary>
public sealed class SetupGuide(ISettingsStore settingsStore)
{
    /// <summary>
    /// <see langword="true"/> as long as nothing was saved, so the guide was neither finished nor skipped yet.
    /// </summary>
    public bool IsFirstRun => !settingsStore.HasSavedSettings;

    /// <summary>
    /// The current settings with the chosen folders and the optional project; creates the library folder when it is missing.
    /// A project with the same folder as an existing one replaces it.
    /// </summary>
    public DepotSettings Apply(DepotSettings current, string libraryRoot, string downloadsDirectory, StrideProject? project)
    {
        var library = libraryRoot.Trim();
        Directory.CreateDirectory(library);

        var projects = current.Projects.ToList();
        if (project is not null)
        {
            var added = new StrideProject(
                project.Name.Trim(),
                project.RootDirectory.Trim(),
                project.SourceTextureDirectory.Trim(),
                project.AssetsDirectory.Trim(),
                project.TextureAssetDirectory.Trim(),
                project.MaterialDirectory.Trim());
            projects.RemoveAll(existing => SameFolder(existing.RootDirectory, added.RootDirectory));
            projects.Add(added);
        }

        return new DepotSettings(library, downloadsDirectory.Trim(), projects);
    }

    private static bool SameFolder(string left, string right) =>
        string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string path) =>
        Path.GetFullPath(path.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
