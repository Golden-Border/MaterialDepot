using System.IO;
using MaterialDepot.App.Resources;
using MaterialDepot.Core.Stride;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// One entry of the worktree list in the library: a worktree of the selected project's repository to import into.
/// </summary>
public sealed record WorktreeOption(ProjectWorktree Target)
{
    /// <summary>
    /// The checked-out branch, or the commit of a detached HEAD.
    /// </summary>
    public string Branch { get; } = Target.Worktree.Branch ?? string.Format(Strings.Library_DetachedHead, Target.Worktree.Commit);

    public string Directory => Target.Worktree.Directory;

    /// <summary>
    /// Branch and folder name, which is also what UI Automation reads out for the list entry.
    /// </summary>
    public string Label { get; } = string.Format(Strings.Library_WorktreeLabel,
        Target.Worktree.Branch ?? string.Format(Strings.Library_DetachedHead, Target.Worktree.Commit),
        Path.GetFileName(Target.Worktree.Directory));

    public override string ToString() => Label;
}
