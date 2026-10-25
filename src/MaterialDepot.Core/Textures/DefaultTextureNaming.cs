namespace MaterialDepot.Core.Textures;

/// <summary>
/// The file name patterns and map names that ship with the depot: textures.com, Poly Haven, ambientCG and Substance exports.
/// </summary>
public static class DefaultTextureNaming
{
    /// <summary>
    /// Poly Haven (<c>aerial_rocks_04_nor_gl_4k</c>), where the resolution follows the map, and everything that ends
    /// in the map: textures.com (<c>TCom_HeavyRust3_1K_albedo</c>), ambientCG (<c>Grass001_1K_Color</c>) and
    /// Substance (<c>Rock_Base Color</c>). The name is matched lazily, so the map is the shortest tail that is a map name.
    /// </summary>
    public static IReadOnlyList<FilePattern> FilePatterns { get; } =
    [
        new("Poly Haven", @"^(?<name>.+?)[_-](?<map>[A-Za-z]+(?:_gl|_ogl)?)[_-](?<res>\d+k)$"),
        new("textures.com, ambientCG, Substance", @"^(?<name>.+?)[_-](?<map>[A-Za-z]+(?: [A-Za-z]+)?(?:[_-]gl|[_-]ogl)?)$"),
    ];

    /// <summary>
    /// DirectX normal maps (NormalDX, nor_dx) are left out on purpose: Stride takes the OpenGL convention.
    /// </summary>
    public static IReadOnlyDictionary<PbrMap, IReadOnlyList<string>> MapNames { get; } = new Dictionary<PbrMap, IReadOnlyList<string>>
    {
        [PbrMap.Albedo] = ["Albedo", "BaseColor", "Color", "Diffuse", "Diff"],
        [PbrMap.Normal] = ["Normal", "NormalGL", "NormalOGL", "Nor_GL"],
        [PbrMap.Roughness] = ["Roughness", "Rough"],
        [PbrMap.Metallic] = ["Metallic", "Metalness"],
        [PbrMap.AmbientOcclusion] = ["AO", "AmbientOcclusion"],
        [PbrMap.Height] = ["Height", "Displacement", "Disp"],
    };
}
