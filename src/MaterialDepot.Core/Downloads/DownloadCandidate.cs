using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Downloads;

/// <summary>
/// The downloaded maps of one set, not yet in the library.
/// </summary>
/// <param name="BaseName">What the map files share, such as <c>TCom_HeavyRust3_1K</c>.</param>
/// <param name="SuggestedName">A set name to start from.</param>
/// <param name="Files">The map files, height maps included.</param>
/// <param name="LibrarySetName">The library set that already holds files of these names, or <see langword="null"/>.</param>
public sealed record DownloadCandidate(string BaseName, string SuggestedName, IReadOnlyList<DownloadedMap> Files, string? LibrarySetName)
{
    public bool IsInLibrary => LibrarySetName is not null;

    public IEnumerable<PbrMap> Maps => Files.Select(file => file.Map).Distinct().Order();

    /// <summary>
    /// The files a move takes away from the download folder: the loose files and the zips the others came in.
    /// </summary>
    public IEnumerable<string> SourceFiles => Files.Select(file => file.Location).Distinct(StringComparer.OrdinalIgnoreCase);
}
