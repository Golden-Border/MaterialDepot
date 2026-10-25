using System.IO.Compression;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Downloads;

public sealed class DownloadScanner : IDownloadScanner
{
    public IReadOnlyList<DownloadCandidate> Scan(string downloads, IReadOnlyCollection<TextureSet> library, MapFileParser parser)
    {
        if (!Directory.Exists(downloads))
        {
            return [];
        }

        var found = new List<(string BaseName, DownloadedMap File)>();
        foreach (var path in Directory.EnumerateFiles(downloads))
        {
            if (Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                found.AddRange(MapsInZip(path, parser));
            }
            else if (parser.TryParse(path, out var mapFile))
            {
                found.Add((mapFile.BaseName, new DownloadedMap(Path.GetFileName(path), mapFile.Map, path, null, new FileInfo(path).Length)));
            }
        }

        var libraryFiles = LibraryFileNames(library);
        return [.. found
            .GroupBy(entry => entry.BaseName, StringComparer.OrdinalIgnoreCase)
            // A download folder holds all kinds of images; a lone "_color.png" is far more often a screenshot than a texture set.
            .Where(group => group.Select(entry => entry.File.Map).Distinct().Count() >= 2)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var files = group.Select(entry => entry.File).OrderBy(file => file.Map).ToList();
                return new DownloadCandidate(group.Key, TextureSetNaming.SuggestName(group.Key), files, LibrarySetHolding(files, libraryFiles));
            })];
    }

    private static IEnumerable<(string BaseName, DownloadedMap File)> MapsInZip(string zipPath, MapFileParser parser)
    {
        List<(string, DownloadedMap)> maps = [];
        try
        {
            using var zip = ZipFile.OpenRead(zipPath);
            foreach (var entry in zip.Entries)
            {
                if (entry.Name.Length > 0 && parser.TryParse(entry.Name, out var mapFile))
                {
                    maps.Add((mapFile.BaseName, new DownloadedMap(entry.Name, mapFile.Map, zipPath, entry.FullName, entry.Length)));
                }
            }
        }
        catch (InvalidDataException)
        {
            // A zip still being downloaded, or not a zip at all: it shows up on the next scan.
        }

        return maps;
    }

    private static Dictionary<string, string> LibraryFileNames(IReadOnlyCollection<TextureSet> library)
    {
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var set in library)
        {
            foreach (var file in Directory.EnumerateFiles(set.Directory))
            {
                names.TryAdd(Path.GetFileName(file), set.Name);
            }
        }

        return names;
    }

    private static string? LibrarySetHolding(IReadOnlyList<DownloadedMap> files, Dictionary<string, string> libraryFiles)
    {
        var sets = files.Select(file => libraryFiles.GetValueOrDefault(file.FileName)).Distinct().ToList();
        return sets is [{ } set] ? set : null;
    }
}
