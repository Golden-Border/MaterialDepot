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

    // Longest first, so "nor_gl" wins over a shorter token that happens to end the name.
    private static readonly (string Token, PbrMap Map)[] mapTokens =
    [
        ("basecolor", PbrMap.Albedo),
        ("albedo", PbrMap.Albedo),
        ("diffuse", PbrMap.Albedo),
        ("diff", PbrMap.Albedo),
        ("nor_gl", PbrMap.Normal),
        ("normal", PbrMap.Normal),
        ("roughness", PbrMap.Roughness),
        ("rough", PbrMap.Roughness),
        ("metallic", PbrMap.Metallic),
        ("metalness", PbrMap.Metallic),
        ("ao", PbrMap.AmbientOcclusion),
        ("height", PbrMap.Height),
        ("displacement", PbrMap.Height),
        ("disp", PbrMap.Height),
    ];

    /// <summary>
    /// The file name without its extension, which an imported copy keeps.
    /// </summary>
    public string Stem => System.IO.Path.GetFileNameWithoutExtension(Path);

    /// <summary>
    /// Recognises textures.com names (<c>TCom_HeavyRust3_1K_albedo.tif</c>) and Poly Haven names
    /// (<c>aerial_rocks_04_diff_4k.jpg</c>). Anything else is not a map file.
    /// </summary>
    public static bool TryParse(string path, out MapFile mapFile)
    {
        mapFile = null!;
        var extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
        if (!imageExtensions.Contains(extension))
        {
            return false;
        }

        var stem = System.IO.Path.GetFileNameWithoutExtension(path);
        var resolution = TrailingResolution().Match(stem);
        var withoutResolution = resolution.Success ? stem[..resolution.Index] : stem;

        foreach (var (token, map) in mapTokens)
        {
            var suffix = "_" + token;
            if (withoutResolution.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) && withoutResolution.Length > suffix.Length)
            {
                mapFile = new MapFile(path, withoutResolution[..^suffix.Length], map);
                return true;
            }
        }

        return false;
    }

    [GeneratedRegex(@"_\d+k$", RegexOptions.IgnoreCase)]
    private static partial Regex TrailingResolution();
}
