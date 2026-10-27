using System.Text.RegularExpressions;

namespace MaterialDepot.Core.Textures;

/// <summary>
/// Recognises the map files of texture sets by name. A file is a map file when it is an image and one of the
/// patterns matches its name without the extension. A pattern has the named groups <c>name</c> (the set, shared by
/// its maps) and <c>map</c> (the map name, which one of the map names has to know), and optionally <c>res</c>
/// (the resolution). The patterns are tried in order, without regard to case, and an invalid one is skipped.
/// </summary>
public sealed class MapFileParser
{
    private static readonly string[] imageExtensions = [".tif", ".tiff", ".png", ".jpg", ".jpeg"];

    private static readonly TimeSpan matchTimeout = TimeSpan.FromMilliseconds(250);

    private readonly List<Regex> patterns;
    private readonly Dictionary<string, PbrMap> mapByToken = new(StringComparer.Ordinal);

    /// <param name="patterns">The patterns, tried in order.</param>
    /// <param name="mapNames">The names each map goes by in file names; compared without case, spaces, hyphens and underscores, so "Base Color", "baseColor" and "base_color" are one name. If two maps share a name, the first wins.</param>
    public MapFileParser(IEnumerable<FilePattern> patterns, IReadOnlyDictionary<PbrMap, IReadOnlyList<string>> mapNames)
    {
        this.patterns = [.. patterns.Where(pattern => Validate(pattern.Regex) is null).Select(pattern => Compile(pattern.Regex))];
        foreach (var (map, names) in mapNames)
        {
            foreach (var name in names)
            {
                mapByToken.TryAdd(Normalize(name), map);
            }
        }
    }

    /// <summary>
    /// The parser for <see cref="DefaultTextureNaming"/>.
    /// </summary>
    public static MapFileParser Default { get; } = new(DefaultTextureNaming.FilePatterns, DefaultTextureNaming.MapNames);

    /// <summary>
    /// Why <paramref name="pattern"/> cannot be used as a file name pattern, or <see langword="null"/> when it is fine.
    /// </summary>
    public static PatternProblem? Validate(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return PatternProblem.Empty;
        }

        Regex regex;
        try
        {
            regex = Compile(pattern);
        }
        catch (ArgumentException)
        {
            return PatternProblem.NotARegex;
        }

        var groups = regex.GetGroupNames();
        return (groups.Contains("name"), groups.Contains("map")) switch
        {
            (true, true) => null,
            (false, true) => PatternProblem.NameGroupMissing,
            (true, false) => PatternProblem.MapGroupMissing,
            _ => PatternProblem.NameAndMapGroupsMissing,
        };
    }

    /// <summary>
    /// Recognises <paramref name="path"/> as a map file; false for anything that is no image or matches no pattern with a known map name.
    /// </summary>
    public bool TryParse(string path, out MapFile mapFile)
    {
        mapFile = null!;
        if (!imageExtensions.Contains(System.IO.Path.GetExtension(path).ToLowerInvariant()))
        {
            return false;
        }

        var parsed = Parse(System.IO.Path.GetFileNameWithoutExtension(path));
        if (parsed?.Map is not { } map)
        {
            return false;
        }

        mapFile = new MapFile(path, parsed.Name, map);
        return true;
    }

    /// <summary>
    /// What the first matching pattern extracts from <paramref name="stem"/>, a file name without its extension.
    /// A match whose map name is unknown is only a fallback, so that the settings can tell why a file is not recognised.
    /// </summary>
    public ParsedFileName? Parse(string stem)
    {
        ParsedFileName? unknownMap = null;
        foreach (var regex in patterns)
        {
            Match match;
            try
            {
                match = regex.Match(stem);
            }
            catch (RegexMatchTimeoutException)
            {
                continue;
            }

            var name = match.Groups["name"].Value;
            var token = match.Groups["map"].Value;
            if (!match.Success || name.Length == 0 || token.Length == 0)
            {
                continue;
            }

            var resolution = match.Groups["res"] is { Success: true } res ? res.Value : null;
            if (mapByToken.TryGetValue(Normalize(token), out var map))
            {
                return new ParsedFileName(name, token, map, resolution);
            }

            unknownMap ??= new ParsedFileName(name, token, null, resolution);
        }

        return unknownMap;
    }

    private static Regex Compile(string pattern) => new(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeout);

    private static string Normalize(string token) => new([.. token.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant)]);
}
