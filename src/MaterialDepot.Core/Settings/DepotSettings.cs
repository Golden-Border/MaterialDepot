using MaterialDepot.Core.Stride;

namespace MaterialDepot.Core.Settings;

/// <summary>
/// The folders the depot works with and the Stride projects it imports into.
/// </summary>
/// <param name="LibraryRoot">The library, one folder per texture set.</param>
/// <param name="DownloadsDirectory">Where the browser puts downloads.</param>
/// <param name="Projects">The import targets.</param>
public sealed record DepotSettings(string LibraryRoot, string DownloadsDirectory, IReadOnlyList<StrideProject> Projects)
{
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
}
