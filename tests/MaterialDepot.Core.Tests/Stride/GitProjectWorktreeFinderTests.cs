using MaterialDepot.Core.Stride;

namespace MaterialDepot.Core.Tests.Stride;

public sealed class GitProjectWorktreeFinderTests : IDisposable
{
    private readonly TempDirectory temp = new();
    private readonly string mainDirectory;
    private readonly GitProjectWorktreeFinder sut = new();

    public GitProjectWorktreeFinderTests()
    {
        mainDirectory = temp.Combine("Game");
        WriteFile(Path.Combine(mainDirectory, ".git", "HEAD"), "ref: refs/heads/develop\n");
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void Find_InTheMainWorktree_MovesTheProjectRootIntoEveryOtherWorktree()
    {
        // Arrange
        var linked = AddWorktree("feature-x", temp.Combine("Game-feature-x"), "ref: refs/heads/feature/x");
        var project = Project(Path.Combine(mainDirectory, "src"));

        // Act
        var worktrees = sut.Find(project);

        // Assert
        worktrees.Count.ShouldBe(2);
        worktrees[0].Worktree.ShouldBe(new GitWorktree(mainDirectory, "develop", null, IsMain: true));
        worktrees[0].IsConfigured.ShouldBeTrue();
        worktrees[0].Project.ShouldBeSameAs(project);
        worktrees[1].Worktree.ShouldBe(new GitWorktree(linked, "feature/x", null, IsMain: false));
        worktrees[1].IsConfigured.ShouldBeFalse();
        worktrees[1].Project.ShouldBe(project with { RootDirectory = Path.Combine(linked, "src") });
    }

    [Fact]
    public void Find_InALinkedWorktree_ConfiguresThatOneAndListsTheMainWorktreeFirst()
    {
        // Arrange
        var linked = AddWorktree("probe", temp.Combine("Game-probe"), "68cb3cb8c549975e14dd15bc47a3dc80d1943552");
        var project = Project(linked);

        // Act
        var worktrees = sut.Find(project);

        // Assert
        worktrees.Select(worktree => worktree.Worktree.Directory).ShouldBe([mainDirectory, linked]);
        worktrees[0].IsConfigured.ShouldBeFalse();
        worktrees[0].Project.RootDirectory.ShouldBe(mainDirectory);
        worktrees[1].IsConfigured.ShouldBeTrue();
        worktrees[1].Worktree.Branch.ShouldBeNull();
        worktrees[1].Worktree.Commit.ShouldBe("68cb3cb");
    }

    [Fact]
    public void Find_WithAWorktreeInsideTheMainOne_ConfiguresTheInnermost()
    {
        // Arrange
        var nested = AddWorktree("nested", Path.Combine(mainDirectory, ".claude", "worktrees", "nested"), "ref: refs/heads/nested");
        var project = Project(Path.Combine(nested, "src"));

        // Act
        var worktrees = sut.Find(project);

        // Assert
        worktrees.Single(worktree => worktree.IsConfigured).Worktree.Directory.ShouldBe(nested);
        worktrees.Single(worktree => worktree.Worktree.IsMain).Project.RootDirectory.ShouldBe(Path.Combine(mainDirectory, "src"));
    }

    [Fact]
    public void Find_LeavesOutAWorktreeWhoseFolderIsGone()
    {
        // Arrange
        var gone = AddWorktree("gone", temp.Combine("Game-gone"), "ref: refs/heads/gone");
        Directory.Delete(gone, recursive: true);

        // Act
        var worktrees = sut.Find(Project(mainDirectory));

        // Assert
        worktrees.Select(worktree => worktree.Worktree.Directory).ShouldBe([mainDirectory]);
    }

    [Fact]
    public void Find_OutsideARepository_IsEmpty()
    {
        // Arrange
        var project = Project(temp.Combine("NoRepository"));
        Directory.CreateDirectory(project.RootDirectory);
        Directory.Delete(Path.Combine(mainDirectory, ".git"), recursive: true);

        // Act & Assert
        sut.Find(project).ShouldBeEmpty();
    }

    /// <summary>
    /// What <c>git worktree add</c> leaves behind: an admin folder in the common <c>.git</c> folder and a <c>.git</c> file
    /// in the worktree, both with absolute paths in forward slashes as Git for Windows writes them.
    /// </summary>
    private string AddWorktree(string name, string directory, string head)
    {
        var admin = Path.Combine(mainDirectory, ".git", "worktrees", name);
        WriteFile(Path.Combine(admin, "gitdir"), Path.Combine(directory, ".git").Replace('\\', '/') + "\n");
        WriteFile(Path.Combine(admin, "commondir"), "../..\n");
        WriteFile(Path.Combine(admin, "HEAD"), head + "\n");
        WriteFile(Path.Combine(directory, ".git"), "gitdir: " + admin.Replace('\\', '/') + "\n");
        return directory;
    }

    private static StrideProject Project(string root) => new("Game", root, "Assets/Textures", "Game/Assets", "Materials", "Materials");

    private static void WriteFile(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }
}
