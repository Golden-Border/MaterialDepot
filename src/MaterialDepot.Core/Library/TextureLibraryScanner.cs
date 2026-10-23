using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Library;

public sealed class TextureLibraryScanner : ITextureLibraryScanner
{
    // When a set lies in a folder both as the downloaded TIFF and as a converted copy, the TIFF is the original.
    private static readonly string[] extensionPreference = [".tif", ".tiff", ".png", ".jpg", ".jpeg"];

    public IReadOnlyList<TextureSet> Scan(string root)
    {
        if (!Directory.Exists(root))
        {
            return [];
        }

        var fullRoot = Path.GetFullPath(root);
        var sets = new List<TextureSet>();
        foreach (var directory in Directory.EnumerateDirectories(fullRoot, "*", SearchOption.AllDirectories).Prepend(fullRoot))
        {
            sets.AddRange(SetsIn(directory, isRoot: directory == fullRoot));
        }

        return [.. sets.OrderBy(set => set.Name, StringComparer.CurrentCultureIgnoreCase)];
    }

    private static IEnumerable<TextureSet> SetsIn(string directory, bool isRoot)
    {
        var byBase = Directory.EnumerateFiles(directory)
            .Select(path => MapFile.TryParse(path, out var mapFile) ? mapFile : null)
            .OfType<MapFile>()
            .GroupBy(mapFile => mapFile.BaseName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => PreferredPerMap(group), StringComparer.OrdinalIgnoreCase);
        if (byBase.Count == 0)
        {
            return [];
        }

        var folderName = Path.GetFileName(directory);
        var mapsClash = byBase.Values.SelectMany(maps => maps.Keys).GroupBy(map => map).Any(group => group.Count() > 1);
        if (!isRoot && !mapsClash)
        {
            var maps = byBase.Values.SelectMany(m => m).ToDictionary(pair => pair.Key, pair => pair.Value);
            return [new TextureSet(folderName, directory, maps)];
        }

        return byBase.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new TextureSet(isRoot ? pair.Key : $"{folderName} ({pair.Key})", directory, pair.Value));
    }

    private static Dictionary<PbrMap, MapFile> PreferredPerMap(IEnumerable<MapFile> files) =>
        files.GroupBy(file => file.Map)
            .ToDictionary(group => group.Key, group => group.OrderBy(file => Rank(file.Path)).First());

    private static int Rank(string path)
    {
        var index = Array.IndexOf(extensionPreference, Path.GetExtension(path).ToLowerInvariant());
        return index < 0 ? extensionPreference.Length : index;
    }
}
