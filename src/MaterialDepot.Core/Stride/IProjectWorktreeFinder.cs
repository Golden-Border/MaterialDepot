namespace MaterialDepot.Core.Stride;

/// <summary>
/// Which worktrees of its Git repository a Stride project can be imported into.
/// </summary>
public interface IProjectWorktreeFinder
{
    /// <summary>
    /// The project in every worktree of the repository its root lies in: the main worktree first, then the others by folder.
    /// A worktree whose folder no longer exists is left out. Empty when the root is not inside a Git repository.
    /// </summary>
    IReadOnlyList<ProjectWorktree> Find(StrideProject project);
}
