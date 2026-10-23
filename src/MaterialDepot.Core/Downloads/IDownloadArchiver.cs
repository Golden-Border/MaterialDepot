namespace MaterialDepot.Core.Downloads;

/// <summary>
/// Puts downloaded sets into the library.
/// </summary>
public interface IDownloadArchiver
{
    /// <summary>
    /// Copies the candidate's map files unchanged into <c>&lt;libraryRoot&gt;/&lt;setName&gt;</c>, unpacking those that
    /// came in a zip. A file already there with the same content is left alone; one with different content is never
    /// overwritten, only reported. With <paramref name="move"/> the loose files and zips are removed afterwards, but only
    /// when every file arrived.
    /// </summary>
    ArchiveResult Archive(DownloadCandidate candidate, string setName, string libraryRoot, bool move);
}
