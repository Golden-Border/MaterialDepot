using MaterialDepot.Core.Stride;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Settings;

/// <summary>
/// The folders the depot works with, the Stride projects it imports into and how texture files are named.
/// </summary>
public sealed record DepotSettings
{
    /// <param name="libraryRoot">The library, one folder per texture set.</param>
    /// <param name="downloadsDirectory">Where the browser puts downloads.</param>
    /// <param name="projects">The import targets.</param>
    /// <param name="filePatterns">Named regular expressions that recognise map files, see <see cref="MapFileParser"/>; <see langword="null"/> (a settings file from before they existed) means <see cref="DefaultTextureNaming.FilePatterns"/>.</param>
    /// <param name="mapNames">The names each map goes by in file names; <see langword="null"/> means <see cref="DefaultTextureNaming.MapNames"/>.</param>
    /// <param name="language">The UI language, one of <see cref="UiLanguages.Supported"/>; <see langword="null"/> follows Windows.</param>
    public DepotSettings(
        string libraryRoot,
        string downloadsDirectory,
        IReadOnlyList<StrideProject> projects,
        IReadOnlyList<FilePattern>? filePatterns = null,
        IReadOnlyDictionary<PbrMap, IReadOnlyList<string>>? mapNames = null,
        string? language = null)
    {
        LibraryRoot = libraryRoot;
        DownloadsDirectory = downloadsDirectory;
        Projects = projects;
        FilePatterns = filePatterns is { Count: > 0 } ? filePatterns : DefaultTextureNaming.FilePatterns;
        MapNames = mapNames is { Count: > 0 } ? mapNames : DefaultTextureNaming.MapNames;
        Language = language;
    }

    public string LibraryRoot { get; }

    public string DownloadsDirectory { get; }

    public IReadOnlyList<StrideProject> Projects { get; }

    public IReadOnlyList<FilePattern> FilePatterns { get; }

    public IReadOnlyDictionary<PbrMap, IReadOnlyList<string>> MapNames { get; }

    public string? Language { get; }

    /// <summary>
    /// The library in a Textures folder and the browser downloads of the user; no Stride project until one is added.
    /// </summary>
    public static DepotSettings CreateDefault(string userProfile)
    {
        return new DepotSettings(
            Path.Combine(userProfile, "Textures"),
            Path.Combine(userProfile, "Downloads"),
            []);
    }

    /// <summary>
    /// The parser for the patterns and map names of these settings.
    /// </summary>
    public MapFileParser CreateParser() => new(FilePatterns, MapNames);
}
