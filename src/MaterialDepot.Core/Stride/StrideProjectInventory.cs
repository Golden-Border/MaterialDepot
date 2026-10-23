namespace MaterialDepot.Core.Stride;

public sealed class StrideProjectInventory : IStrideProjectInventory
{
    private const string SourcePrefix = "Source: !file ";

    public IReadOnlySet<string> ImportedStems(StrideProject project)
    {
        var stems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(project.AssetsPath))
        {
            return stems;
        }

        foreach (var texture in Directory.EnumerateFiles(project.AssetsPath, "*.sdtex", SearchOption.AllDirectories))
        {
            foreach (var line in File.ReadLines(texture))
            {
                if (line.StartsWith(SourcePrefix, StringComparison.Ordinal))
                {
                    stems.Add(Path.GetFileNameWithoutExtension(line[SourcePrefix.Length..].Trim()));
                    break;
                }
            }
        }

        return stems;
    }
}
