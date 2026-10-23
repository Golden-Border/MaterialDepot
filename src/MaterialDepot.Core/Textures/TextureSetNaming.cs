using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MaterialDepot.Core.Textures;

/// <summary>
/// What the base name of a texture set tells about it.
/// </summary>
public static partial class TextureSetNaming
{
    private static readonly string[] storePrefixes = ["TexturesCom_", "TCom_"];

    /// <summary>
    /// A readable set name to start from: <c>TCom_HeavyRust3_1K</c> gives <c>Heavy Rust 3</c>,
    /// <c>aerial_rocks_04</c> gives <c>Aerial Rocks 04</c>. textures.com's product name is not in its
    /// file names, so this is a proposal for the user to correct, never a lookup.
    /// </summary>
    public static string SuggestName(string baseName)
    {
        var prefix = storePrefixes.FirstOrDefault(p => baseName.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        var rest = prefix is null ? baseName : baseName[prefix.Length..];
        var tokens = rest.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => !Resolution().IsMatch(token) && !TileSize().IsMatch(token))
            .ToList();

        // textures.com puts a category first (TCom_Metal_GoldOld_1K); the name is what follows it.
        if (prefix is not null && tokens.Count > 1)
        {
            tokens.RemoveAt(0);
        }

        var words = tokens.SelectMany(SplitWords).Select(Capitalize);
        var name = string.Join(' ', words);
        return name.Length > 0 ? name : baseName;
    }

    /// <summary>
    /// The tile size in metres when the base name carries one, as textures.com's <c>_4x4_</c> or <c>_3.5x3.5_</c> does.
    /// </summary>
    public static double? TileSizeMeters(string baseName)
    {
        foreach (var token in baseName.Split('_'))
        {
            var match = TileSize().Match(token);
            if (match.Success)
            {
                return double.Parse(match.Groups["width"].Value, CultureInfo.InvariantCulture);
            }
        }

        return null;
    }

    private static IEnumerable<string> SplitWords(string token)
    {
        var word = new StringBuilder();
        for (var i = 0; i < token.Length; i++)
        {
            var current = token[i];
            var previous = i > 0 ? token[i - 1] : '\0';
            var startsWord = word.Length > 0
                && ((char.IsUpper(current) && char.IsLower(previous))
                    || (char.IsDigit(current) != char.IsDigit(previous)));
            if (startsWord)
            {
                yield return word.ToString();
                word.Clear();
            }

            word.Append(current);
        }

        if (word.Length > 0)
        {
            yield return word.ToString();
        }
    }

    private static string Capitalize(string word) => char.ToUpperInvariant(word[0]) + word[1..];

    [GeneratedRegex(@"^\d+k$", RegexOptions.IgnoreCase)]
    private static partial Regex Resolution();

    [GeneratedRegex(@"^(?<width>\d+(\.\d+)?)x\d+(\.\d+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex TileSize();
}
