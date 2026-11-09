namespace MaterialDepot.Core.Stride;

/// <summary>
/// Reads the worktrees straight from the repository's administrative files, so no Git installation is needed:
/// the main worktree is the folder around the common <c>.git</c> folder, every other one is listed under
/// <c>.git/worktrees/&lt;name&gt;</c> with a <c>gitdir</c> file that points at the <c>.git</c> file in its folder.
/// </summary>
public sealed class GitProjectWorktreeFinder : IProjectWorktreeFinder
{
    private const string DotGit = ".git";
    private const string GitDirPrefix = "gitdir:";
    private const string RefPrefix = "ref:";
    private const string BranchRefPrefix = "refs/heads/";
    private const int ShortCommitLength = 7;

    public IReadOnlyList<ProjectWorktree> Find(StrideProject project)
    {
        var root = Normalize(project.RootDirectory);
        if (FindCommonDirectory(root) is not { } commonDirectory)
        {
            return [];
        }

        var worktrees = ReadWorktrees(commonDirectory);
        var home = worktrees.Where(worktree => Contains(worktree.Directory, root)).MaxBy(worktree => worktree.Directory.Length);
        if (home is null)
        {
            return [];
        }

        var relativeRoot = Path.GetRelativePath(home.Directory, root);
        return
        [
            .. worktrees.Select(worktree => worktree == home
                ? new ProjectWorktree(worktree, project, IsConfigured: true)
                : new ProjectWorktree(worktree, project with { RootDirectory = Normalize(Path.Combine(worktree.Directory, relativeRoot)) }, IsConfigured: false)),
        ];
    }

    /// <summary>
    /// The repository's common Git folder, found from the nearest <c>.git</c> folder or file at or above <paramref name="directory"/>.
    /// </summary>
    private static string? FindCommonDirectory(string directory)
    {
        for (var current = new DirectoryInfo(directory); current is not null; current = current.Parent)
        {
            var dotGit = Path.Combine(current.FullName, DotGit);
            if (Directory.Exists(dotGit))
            {
                return CommonDirectoryOf(dotGit);
            }

            if (File.Exists(dotGit))
            {
                return ReadFirstLine(dotGit) is { } line && line.StartsWith(GitDirPrefix, StringComparison.Ordinal)
                    ? CommonDirectoryOf(Normalize(Path.Combine(current.FullName, line[GitDirPrefix.Length..].Trim())))
                    : null;
            }
        }

        return null;
    }

    /// <summary>
    /// A linked worktree's Git folder names the common one in its <c>commondir</c> file; the main one is the common one.
    /// </summary>
    private static string CommonDirectoryOf(string gitDirectory) =>
        ReadFirstLine(Path.Combine(gitDirectory, "commondir")) is { Length: > 0 } common
            ? Normalize(Path.Combine(gitDirectory, common))
            : gitDirectory;

    private static List<GitWorktree> ReadWorktrees(string commonDirectory)
    {
        var worktrees = new List<GitWorktree>();
        if (string.Equals(Path.GetFileName(commonDirectory), DotGit, StringComparison.OrdinalIgnoreCase)
            && Path.GetDirectoryName(commonDirectory) is { } mainDirectory
            && Directory.Exists(mainDirectory))
        {
            worktrees.Add(Worktree(mainDirectory, commonDirectory, isMain: true));
        }

        var adminRoot = Path.Combine(commonDirectory, "worktrees");
        if (!Directory.Exists(adminRoot))
        {
            return worktrees;
        }

        var linked = new List<GitWorktree>();
        foreach (var admin in Directory.EnumerateDirectories(adminRoot))
        {
            if (ReadFirstLine(Path.Combine(admin, "gitdir")) is not { Length: > 0 } dotGitPath
                || Path.GetDirectoryName(Normalize(Path.Combine(admin, dotGitPath))) is not { } directory
                || !Directory.Exists(directory))
            {
                continue;
            }

            linked.Add(Worktree(directory, admin, isMain: false));
        }

        worktrees.AddRange(linked.OrderBy(worktree => worktree.Directory, StringComparer.OrdinalIgnoreCase));
        return worktrees;
    }

    private static GitWorktree Worktree(string directory, string gitDirectory, bool isMain)
    {
        var head = ReadFirstLine(Path.Combine(gitDirectory, "HEAD")) ?? string.Empty;
        if (head.StartsWith(RefPrefix, StringComparison.Ordinal))
        {
            var reference = head[RefPrefix.Length..].Trim();
            var branch = reference.StartsWith(BranchRefPrefix, StringComparison.Ordinal) ? reference[BranchRefPrefix.Length..] : reference;
            return new GitWorktree(directory, branch, Commit: null, isMain);
        }

        var commit = head.Length > ShortCommitLength ? head[..ShortCommitLength] : head;
        return new GitWorktree(directory, Branch: null, commit.Length > 0 ? commit : null, isMain);
    }

    private static string? ReadFirstLine(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadLines(path).FirstOrDefault()?.Trim() : null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static bool Contains(string directory, string path) =>
        string.Equals(directory, path, StringComparison.OrdinalIgnoreCase)
        || path.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
}
