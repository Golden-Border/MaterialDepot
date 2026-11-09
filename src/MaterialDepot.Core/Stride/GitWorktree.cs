namespace MaterialDepot.Core.Stride;

/// <summary>
/// One working tree of a Git repository: the main one or one added with <c>git worktree add</c>.
/// </summary>
/// <param name="Directory">The top folder of the working tree.</param>
/// <param name="Branch">The checked-out branch without <c>refs/heads/</c>; <see langword="null"/> on a detached HEAD.</param>
/// <param name="Commit">The abbreviated commit of a detached HEAD; <see langword="null"/> on a branch.</param>
/// <param name="IsMain">Whether this is the repository's main working tree.</param>
public sealed record GitWorktree(string Directory, string? Branch, string? Commit, bool IsMain);
