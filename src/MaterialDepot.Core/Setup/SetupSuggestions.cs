using MaterialDepot.Core.Stride;

namespace MaterialDepot.Core.Setup;

/// <summary>
/// Starting values for a Stride project the user just picked.
/// </summary>
public static class SetupSuggestions
{
    public const string SourceTextureDirectory = "Assets/Textures";
    public const string TextureAssetDirectory = "Textures";
    public const string MaterialDirectory = "Materials";

    private const int maxPackageDepth = 3;

    private static readonly HashSet<string> skippedFolders = new(["bin", "obj", "node_modules"], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The project folder's name as display name; the asset folder next to the <c>.sdpkg</c> if there is one, else <c>Assets</c>.
    /// </summary>
    public static StrideProject ForProject(string rootDirectory)
    {
        var root = rootDirectory.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return new StrideProject(Path.GetFileName(root), root, SourceTextureDirectory, FindAssetsDirectory(root), TextureAssetDirectory, MaterialDirectory);
    }

    private static string FindAssetsDirectory(string root)
    {
        if (!Directory.Exists(root) || FindPackageDirectory(root, 0) is not { } packageDirectory)
        {
            return "Assets";
        }

        var relative = Path.GetRelativePath(root, packageDirectory).Replace('\\', '/');
        return relative == "." ? "Assets" : relative + "/Assets";
    }

    /// <summary>
    /// The folder with a <c>.sdpkg</c>; one nearer to the root wins over one deeper down.
    /// </summary>
    private static string? FindPackageDirectory(string directory, int depth)
    {
        try
        {
            if (Directory.EnumerateFiles(directory, "*.sdpkg").Any())
            {
                return directory;
            }

            if (depth >= maxPackageDepth)
            {
                return null;
            }

            var children = Directory.EnumerateDirectories(directory)
                .Where(child => !Path.GetFileName(child).StartsWith('.') && !skippedFolders.Contains(Path.GetFileName(child)))
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return children.FirstOrDefault(child => Directory.EnumerateFiles(child, "*.sdpkg").Any())
                ?? children.Select(child => FindPackageDirectory(child, depth + 1)).FirstOrDefault(found => found is not null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
