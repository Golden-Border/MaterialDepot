using MaterialDepot.Core.Stride;

namespace MaterialDepot.Core.Setup;

/// <summary>
/// Checks the folders the setup guide collects.
/// </summary>
public static class SetupValidator
{
    public static FolderStatus CheckFolder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return FolderStatus.Empty;
        }

        var trimmed = path.Trim();
        if (!IsAbsolute(trimmed))
        {
            return FolderStatus.Invalid;
        }

        if (File.Exists(trimmed))
        {
            return FolderStatus.IsFile;
        }

        return Directory.Exists(trimmed) ? FolderStatus.Exists : FolderStatus.Missing;
    }

    /// <summary>
    /// Everything wrong with a project entry; empty when it can be saved.
    /// </summary>
    public static IReadOnlyList<ProjectProblem> CheckProject(StrideProject project)
    {
        var problems = new List<ProjectProblem>();
        if (string.IsNullOrWhiteSpace(project.Name))
        {
            problems.Add(ProjectProblem.NameMissing);
        }

        switch (CheckFolder(project.RootDirectory))
        {
            case FolderStatus.Empty or FolderStatus.Missing:
                problems.Add(ProjectProblem.RootMissing);
                break;
            case FolderStatus.Invalid or FolderStatus.IsFile:
                problems.Add(ProjectProblem.RootInvalid);
                break;
        }

        if (!IsRelative(project.SourceTextureDirectory, allowEmpty: false))
        {
            problems.Add(ProjectProblem.SourceTextureDirectoryInvalid);
        }

        if (!IsRelative(project.AssetsDirectory, allowEmpty: true))
        {
            problems.Add(ProjectProblem.AssetsDirectoryInvalid);
        }

        if (!IsRelative(project.TextureAssetDirectory, allowEmpty: true))
        {
            problems.Add(ProjectProblem.TextureAssetDirectoryInvalid);
        }

        if (!IsRelative(project.MaterialDirectory, allowEmpty: true))
        {
            problems.Add(ProjectProblem.MaterialDirectoryInvalid);
        }

        return problems;
    }

    private static bool IsAbsolute(string path)
    {
        try
        {
            return Path.IsPathFullyQualified(path) && Path.GetFullPath(path).Length > 0;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    /// <summary>
    /// A path below its base folder: not rooted and without a way out through <c>..</c>.
    /// </summary>
    private static bool IsRelative(string? path, bool allowEmpty)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return allowEmpty;
        }

        var trimmed = path.Trim();
        if (Path.IsPathRooted(trimmed) || trimmed.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return false;
        }

        return !trimmed.Split('/', '\\').Contains("..");
    }
}
