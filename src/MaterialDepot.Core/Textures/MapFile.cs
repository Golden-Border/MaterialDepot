using System.Text.RegularExpressions;

namespace MaterialDepot.Core.Textures;

/// <summary>
/// An image file that holds one map of a texture set, recognised by its name.
/// </summary>
/// <param name="Path">Full path of the file.</param>
/// <param name="BaseName">The name without the map token, the resolution token after it and the extension; what the maps of one set share.</param>
/// <param name="Map">The map the file holds.</param>
public sealed partial record MapFile(string Path, string BaseName, PbrMap Map)
{
    private static readonly string[] imageExtensions = [".tif", ".tiff", ".png", ".jpg", ".jpeg"];

    // Compared without case, spaces, hyphens and underscores, so "Base Color", "baseColor" and "basecolor" are one token.
    // DirectX normal maps (NormalDX, nor_dx) are left out on purpose: Stride takes the OpenGL convention.
    private static readonly Dictionary<string, PbrMap> mapTokens = new(StringComparer.Ordinal)
    {
        ["albedo"] = PbrMap.Albedo,
        ["basecolor"] = PbrMap.Albedo,
        ["color"] = PbrMap.Albedo,
        ["diffuse"] = PbrMap.Albedo,
        ["diff"] = PbrMap.Albedo,
        ["normal"] = PbrMap.Normal,
        ["normalgl"] = PbrMap.Normal,
        ["normalogl"] = PbrMap.Normal,
        ["norgl"] = PbrMap.Normal,
        ["roughness"] = PbrMap.Roughness,
        ["rough"] = PbrMap.Roughness,
        ["metallic"] = PbrMap.Metallic,
        ["metalness"] = PbrMap.Metallic,
        ["ao"] = PbrMap.AmbientOcclusion,
        ["ambientocclusion"] = PbrMap.AmbientOcclusion,
        ["height"] = PbrMap.Height,
        ["displacement"] = PbrMap.Height,
        ["disp"] = PbrMap.Height,
    };

    /// <summary>
    /// The file name without its extension, which an imported copy keeps.
    /// </summary>
    public string Stem => System.IO.Path.GetFileNameWithoutExtension(Path);

    /// <summary>
    /// Recognises the map token at the end of the name, after the last <c>_</c> or <c>-</c> and before an optional
    /// resolution: textures.com (<c>TCom_HeavyRust3_1K_albedo.tif</c>), Poly Haven (<c>aerial_rocks_04_nor_gl_4k.png</c>),
    /// ambientCG (<c>Grass001_1K_Color.png</c>) and Substance exports (<c>Rock_Base Color.png</c>). Anything else is not a map file.
    /// </summary>
    public static bool TryParse(string path, out MapFile mapFile)
    {
        mapFile = null!;
        if (!imageExtensions.Contains(System.IO.Path.GetExtension(path).ToLowerInvariant()))
        {
            return false;
        }

        var stem = System.IO.Path.GetFileNameWithoutExtension(path);
        var resolution = TrailingResolution().Match(stem);
        var name = resolution.Success ? stem[..resolution.Index] : stem;

        foreach (var tokenStart in TokenStarts(name))
        {
            if (tokenStart > 1 && mapTokens.TryGetValue(Normalize(name[tokenStart..]), out var map))
            {
                mapFile = new MapFile(path, name[..(tokenStart - 1)], map);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Where the last token, and the last two joined by an underscore (<c>nor_gl</c>), start; the longer one first.
    /// </summary>
    private static IEnumerable<int> TokenStarts(string name)
    {
        var last = name.LastIndexOfAny(['_', '-']);
        if (last < 0)
        {
            yield break;
        }

        var beforeLast = last > 0 ? name.LastIndexOf('_', last - 1) : -1;
        if (beforeLast >= 0)
        {
            yield return beforeLast + 1;
        }

        yield return last + 1;
    }

    private static string Normalize(string token) => new([.. token.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant)]);

    [GeneratedRegex(@"[_-]\d+k$", RegexOptions.IgnoreCase)]
    private static partial Regex TrailingResolution();
}
