using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Downloads;

/// <summary>
/// Finds texture sets in the download folder.
/// </summary>
public interface IDownloadScanner
{
    /// <summary>
    /// The map files directly in <paramref name="downloads"/> and inside its zips, grouped by base name; a group with
    /// only one kind of map is left out. A candidate whose file names all appear in one set of <paramref name="library"/>
    /// is marked as already there. Which files are map files is up to <paramref name="parser"/>.
    /// </summary>
    IReadOnlyList<DownloadCandidate> Scan(string downloads, IReadOnlyCollection<TextureSet> library, MapFileParser parser);
}
