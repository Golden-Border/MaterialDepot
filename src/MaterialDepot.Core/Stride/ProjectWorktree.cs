namespace MaterialDepot.Core.Stride;

/// <summary>
/// A Stride project as it lies in one worktree of its repository.
/// </summary>
/// <param name="Worktree">The worktree.</param>
/// <param name="Project">The configured project with its root moved into <paramref name="Worktree"/>; every other folder stays relative.</param>
/// <param name="IsConfigured">Whether this is the worktree the configured project root lies in.</param>
public sealed record ProjectWorktree(GitWorktree Worktree, StrideProject Project, bool IsConfigured);
