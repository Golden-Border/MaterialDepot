namespace MaterialDepot.Core.Downloads;

/// <summary>
/// What putting a set into the library did.
/// </summary>
/// <param name="SetDirectory">The set's folder in the library.</param>
/// <param name="Copied">Files written.</param>
/// <param name="AlreadyThere">Files left alone because the library held the same content.</param>
/// <param name="Conflicts">Files left alone because the library held different content under the same name.</param>
/// <param name="Removed">Download files deleted by a move.</param>
public sealed record ArchiveResult(
    string SetDirectory,
    IReadOnlyList<string> Copied,
    IReadOnlyList<string> AlreadyThere,
    IReadOnlyList<string> Conflicts,
    IReadOnlyList<string> Removed);
