using System.Globalization;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Stride;

/// <summary>
/// Writes the YAML of texture and material assets the way Game Studio writes them for a textures.com set.
/// </summary>
internal static class StrideAssetText
{
    private const string NewLine = "\r\n";

    /// <summary>
    /// The <c>.sdtex</c> of one map. <paramref name="sourcePath"/> is relative to the asset's own folder.
    /// </summary>
    public static string Texture(Guid id, string sourcePath, PbrMap map)
    {
        List<string> lines =
        [
            "!Texture",
            $"Id: {id}",
            "SerializedVersion: {Stride: 2.0.0.0}",
            "Tags: []",
            $"Source: !file {sourcePath.Replace('\\', '/')}",
        ];
        lines.AddRange(map switch
        {
            PbrMap.Albedo => ["Type: !ColorTextureType", "    ColorKeyColor: {R: 255, G: 0, B: 255, A: 255}"],
            PbrMap.Normal => ["Type: !NormalMapTextureType {}"],
            PbrMap.Roughness or PbrMap.Metallic or PbrMap.AmbientOcclusion => ["Type: !GrayscaleTextureType {}"],
            _ => throw new ArgumentOutOfRangeException(nameof(map), map, "Stride gets no texture asset for this map."),
        });
        return Join(lines);
    }

    /// <summary>
    /// The <c>.sdmat</c> that wires the given textures. Albedo, normal and roughness are required;
    /// metallic adds the metalness feature, ambient occlusion the occlusion feature.
    /// </summary>
    public static string Material(Guid id, IReadOnlyDictionary<PbrMap, StrideAssetReference> textures, double uvScale)
    {
        List<string> lines =
        [
            "!MaterialAsset",
            $"Id: {id}",
            "SerializedVersion: {Stride: 2.0.0.0}",
            "Tags: []",
            "Attributes:",
            "    Surface: !MaterialNormalMapFeature",
            .. ColorTexture("NormalMap", textures[PbrMap.Normal], "{R: 1.0, G: 1.0, B: 1.0, A: 1.0}"),
            "    MicroSurface: !MaterialGlossinessMapFeature",
            .. ScalarTexture("GlossinessMap", textures[PbrMap.Roughness]),
            "        Invert: true",
            "    Diffuse: !MaterialDiffuseMapFeature",
            .. ColorTexture("DiffuseMap", textures[PbrMap.Albedo], "{R: 1.0, G: 0.8392157, B: 0.43529412, A: 1.0}"),
            "    DiffuseModel: !MaterialDiffuseLambertModelFeature {}",
        ];

        if (textures.TryGetValue(PbrMap.Metallic, out var metallic))
        {
            lines.Add("    Specular: !MaterialMetalnessMapFeature");
            lines.AddRange(ScalarTexture("MetalnessMap", metallic));
            lines.AddRange(
            [
                "    SpecularModel: !MaterialSpecularMicrofacetModelFeature",
                "        Fresnel: !MaterialSpecularMicrofacetFresnelSchlick {}",
                "        Visibility: !MaterialSpecularMicrofacetVisibilitySmithSchlickGGX {}",
                "        NormalDistribution: !MaterialSpecularMicrofacetNormalDistributionGGX {}",
                "        Environment: !MaterialSpecularMicrofacetEnvironmentGGXLUT {}",
            ]);
        }

        if (textures.TryGetValue(PbrMap.AmbientOcclusion, out var ambientOcclusion))
        {
            lines.Add("    Occlusion: !MaterialOcclusionMapFeature");
            lines.AddRange(ScalarTexture("AmbientOcclusionMap", ambientOcclusion));
            lines.AddRange(["        DirectLightingFactor: !ComputeFloat", "            Value: 0.0"]);
            lines.AddRange(ScalarTexture("CavityMap", ambientOcclusion));
            lines.AddRange(
            [
                "        DiffuseCavity: !ComputeFloat",
                "            Value: 1.0",
                "        SpecularCavity: !ComputeFloat",
                "            Value: 1.0",
            ]);
        }

        var uv = FormatNumber(uvScale);
        lines.AddRange(["    Overrides:", $"        UVScale: {{X: {uv}, Y: {uv}}}", "Layers: {}"]);
        return Join(lines);
    }

    /// <summary>
    /// The <c>Id</c> in the header of an asset file, or <see langword="null"/> when the text has none.
    /// </summary>
    public static Guid? ReadId(string assetText)
    {
        foreach (var line in assetText.Split('\n'))
        {
            if (line.StartsWith("Id: ", StringComparison.Ordinal) && Guid.TryParse(line[4..].Trim(), out var id))
            {
                return id;
            }
        }

        return null;
    }

    /// <summary>
    /// A number as Stride writes it: invariant, always with a decimal point (<c>1.0</c>, <c>2.5</c>).
    /// </summary>
    public static string FormatNumber(double value) => value.ToString("0.0###", CultureInfo.InvariantCulture);

    private static string[] ColorTexture(string key, StrideAssetReference texture, string fallback) =>
    [
        $"        {key}: !ComputeTextureColor",
        $"            Texture: {texture}",
        "            FallbackValue:",
        $"                Value: {fallback}",
        "            Scale: {X: 1.0, Y: 1.0}",
        "            Offset: {X: 0.0, Y: 0.0}",
        "            Swizzle: null",
    ];

    private static string[] ScalarTexture(string key, StrideAssetReference texture) =>
    [
        $"        {key}: !ComputeTextureScalar",
        $"            Texture: {texture}",
        "            FallbackValue:",
        "                Value: 1.0",
        "            Scale: {X: 1.0, Y: 1.0}",
        "            Offset: {X: 0.0, Y: 0.0}",
    ];

    private static string Join(IEnumerable<string> lines) => string.Join(NewLine, lines) + NewLine;
}
