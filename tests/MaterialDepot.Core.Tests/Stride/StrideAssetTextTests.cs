using MaterialDepot.Core.Stride;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Stride;

public class StrideAssetTextTests
{
    private static readonly Guid albedoId = Guid.Parse("d9bb8f83-fb51-4140-96c8-70019414352a");
    private static readonly Guid normalId = Guid.Parse("170e3f81-6775-45cc-be96-de5e02fd2d55");
    private static readonly Guid roughnessId = Guid.Parse("fc034dc7-2fb2-4f14-93ec-503b51e8984f");
    private static readonly Guid metallicId = Guid.Parse("d5d94330-8935-414e-9ef2-d8866db4d339");
    private static readonly Guid aoId = Guid.Parse("3c3613df-1975-47b0-9020-fe44a5e5836b");

    [Fact]
    public void Texture_WritesAnAlbedoAsColorTextureWithTheColorKey()
    {
        // Act
        var text = StrideAssetText.Texture(albedoId, @"..\..\..\..\..\Assets\Textures\Heavy Rust 3\TCom_HeavyRust3_1K_albedo.png", PbrMap.Albedo);

        // Assert
        text.ShouldBe(
            "!Texture\r\n"
            + "Id: d9bb8f83-fb51-4140-96c8-70019414352a\r\n"
            + "SerializedVersion: {Stride: 2.0.0.0}\r\n"
            + "Tags: []\r\n"
            + "Source: !file ../../../../../Assets/Textures/Heavy Rust 3/TCom_HeavyRust3_1K_albedo.png\r\n"
            + "Type: !ColorTextureType\r\n"
            + "    ColorKeyColor: {R: 255, G: 0, B: 255, A: 255}\r\n");
    }

    [Theory]
    [InlineData(PbrMap.Normal, "Type: !NormalMapTextureType {}")]
    [InlineData(PbrMap.Roughness, "Type: !GrayscaleTextureType {}")]
    [InlineData(PbrMap.Metallic, "Type: !GrayscaleTextureType {}")]
    [InlineData(PbrMap.AmbientOcclusion, "Type: !GrayscaleTextureType {}")]
    public void Texture_EndsWithTheTypeOfTheMap(PbrMap map, string typeLine)
    {
        // Act
        var text = StrideAssetText.Texture(normalId, "map.png", map);

        // Assert
        text.ShouldEndWith("Source: !file map.png\r\n" + typeLine + "\r\n");
    }

    [Fact]
    public void Texture_RefusesAHeightMap()
    {
        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => StrideAssetText.Texture(normalId, "map.png", PbrMap.Height));
    }

    [Fact]
    public void Material_WithAllMaps_WiresEveryFeature()
    {
        // Act
        var text = StrideAssetText.Material(Guid.Parse("8e7a48ea-d5dd-4229-9f8a-40cee92cef84"), AllTextures(), 2.5);

        // Assert
        text.ShouldBe(ForgedIron);
    }

    [Fact]
    public void Material_WithoutMetallicAndAo_LeavesOutSpecularAndOcclusion()
    {
        // Arrange
        var textures = AllTextures();
        textures.Remove(PbrMap.Metallic);
        textures.Remove(PbrMap.AmbientOcclusion);

        // Act
        var text = StrideAssetText.Material(Guid.NewGuid(), textures, 1);

        // Assert
        text.ShouldNotContain("Specular");
        text.ShouldNotContain("Occlusion");
        text.ShouldContain("    DiffuseModel: !MaterialDiffuseLambertModelFeature {}\r\n    Overrides:\r\n        UVScale: {X: 1.0, Y: 1.0}\r\n");
    }

    [Theory]
    [InlineData(1.0, "1.0")]
    [InlineData(0.25, "0.25")]
    [InlineData(2.5, "2.5")]
    [InlineData(1.0 / 3.0, "0.3333")]
    public void FormatNumber_WritesStrideStyleDecimals(double value, string expected)
    {
        // Act & Assert
        StrideAssetText.FormatNumber(value).ShouldBe(expected);
    }

    [Fact]
    public void ReadId_FindsTheHeaderId()
    {
        // Act
        var id = StrideAssetText.ReadId("!MaterialAsset\r\nId: 8e7a48ea-d5dd-4229-9f8a-40cee92cef84\r\nTags: []\r\n");

        // Assert
        id.ShouldBe(Guid.Parse("8e7a48ea-d5dd-4229-9f8a-40cee92cef84"));
    }

    [Fact]
    public void ReadId_WithoutHeaderId_IsNull()
    {
        // Act & Assert
        StrideAssetText.ReadId("!Texture\r\nTags: []\r\n").ShouldBeNull();
    }

    private static Dictionary<PbrMap, StrideAssetReference> AllTextures() => new()
    {
        [PbrMap.Albedo] = new(albedoId, "Materials/Heavy Rust 3/TCom_HeavyRust3_1K_albedo"),
        [PbrMap.Normal] = new(normalId, "Materials/Heavy Rust 3/TCom_HeavyRust3_1K_normal"),
        [PbrMap.Roughness] = new(roughnessId, "Materials/Heavy Rust 3/TCom_HeavyRust3_1K_roughness"),
        [PbrMap.Metallic] = new(metallicId, "Materials/Heavy Rust 3/TCom_HeavyRust3_1K_metallic"),
        [PbrMap.AmbientOcclusion] = new(aoId, "Materials/Heavy Rust 3/TCom_HeavyRust3_1K_ao"),
    };

    // A Forged Iron.sdmat as checked in Game Studio.
    private const string ForgedIron =
        "!MaterialAsset\r\n"
        + "Id: 8e7a48ea-d5dd-4229-9f8a-40cee92cef84\r\n"
        + "SerializedVersion: {Stride: 2.0.0.0}\r\n"
        + "Tags: []\r\n"
        + "Attributes:\r\n"
        + "    Surface: !MaterialNormalMapFeature\r\n"
        + "        NormalMap: !ComputeTextureColor\r\n"
        + "            Texture: 170e3f81-6775-45cc-be96-de5e02fd2d55:Materials/Heavy Rust 3/TCom_HeavyRust3_1K_normal\r\n"
        + "            FallbackValue:\r\n"
        + "                Value: {R: 1.0, G: 1.0, B: 1.0, A: 1.0}\r\n"
        + "            Scale: {X: 1.0, Y: 1.0}\r\n"
        + "            Offset: {X: 0.0, Y: 0.0}\r\n"
        + "            Swizzle: null\r\n"
        + "    MicroSurface: !MaterialGlossinessMapFeature\r\n"
        + "        GlossinessMap: !ComputeTextureScalar\r\n"
        + "            Texture: fc034dc7-2fb2-4f14-93ec-503b51e8984f:Materials/Heavy Rust 3/TCom_HeavyRust3_1K_roughness\r\n"
        + "            FallbackValue:\r\n"
        + "                Value: 1.0\r\n"
        + "            Scale: {X: 1.0, Y: 1.0}\r\n"
        + "            Offset: {X: 0.0, Y: 0.0}\r\n"
        + "        Invert: true\r\n"
        + "    Diffuse: !MaterialDiffuseMapFeature\r\n"
        + "        DiffuseMap: !ComputeTextureColor\r\n"
        + "            Texture: d9bb8f83-fb51-4140-96c8-70019414352a:Materials/Heavy Rust 3/TCom_HeavyRust3_1K_albedo\r\n"
        + "            FallbackValue:\r\n"
        + "                Value: {R: 1.0, G: 0.8392157, B: 0.43529412, A: 1.0}\r\n"
        + "            Scale: {X: 1.0, Y: 1.0}\r\n"
        + "            Offset: {X: 0.0, Y: 0.0}\r\n"
        + "            Swizzle: null\r\n"
        + "    DiffuseModel: !MaterialDiffuseLambertModelFeature {}\r\n"
        + "    Specular: !MaterialMetalnessMapFeature\r\n"
        + "        MetalnessMap: !ComputeTextureScalar\r\n"
        + "            Texture: d5d94330-8935-414e-9ef2-d8866db4d339:Materials/Heavy Rust 3/TCom_HeavyRust3_1K_metallic\r\n"
        + "            FallbackValue:\r\n"
        + "                Value: 1.0\r\n"
        + "            Scale: {X: 1.0, Y: 1.0}\r\n"
        + "            Offset: {X: 0.0, Y: 0.0}\r\n"
        + "    SpecularModel: !MaterialSpecularMicrofacetModelFeature\r\n"
        + "        Fresnel: !MaterialSpecularMicrofacetFresnelSchlick {}\r\n"
        + "        Visibility: !MaterialSpecularMicrofacetVisibilitySmithSchlickGGX {}\r\n"
        + "        NormalDistribution: !MaterialSpecularMicrofacetNormalDistributionGGX {}\r\n"
        + "        Environment: !MaterialSpecularMicrofacetEnvironmentGGXLUT {}\r\n"
        + "    Occlusion: !MaterialOcclusionMapFeature\r\n"
        + "        AmbientOcclusionMap: !ComputeTextureScalar\r\n"
        + "            Texture: 3c3613df-1975-47b0-9020-fe44a5e5836b:Materials/Heavy Rust 3/TCom_HeavyRust3_1K_ao\r\n"
        + "            FallbackValue:\r\n"
        + "                Value: 1.0\r\n"
        + "            Scale: {X: 1.0, Y: 1.0}\r\n"
        + "            Offset: {X: 0.0, Y: 0.0}\r\n"
        + "        DirectLightingFactor: !ComputeFloat\r\n"
        + "            Value: 0.0\r\n"
        + "        CavityMap: !ComputeTextureScalar\r\n"
        + "            Texture: 3c3613df-1975-47b0-9020-fe44a5e5836b:Materials/Heavy Rust 3/TCom_HeavyRust3_1K_ao\r\n"
        + "            FallbackValue:\r\n"
        + "                Value: 1.0\r\n"
        + "            Scale: {X: 1.0, Y: 1.0}\r\n"
        + "            Offset: {X: 0.0, Y: 0.0}\r\n"
        + "        DiffuseCavity: !ComputeFloat\r\n"
        + "            Value: 1.0\r\n"
        + "        SpecularCavity: !ComputeFloat\r\n"
        + "            Value: 1.0\r\n"
        + "    Overrides:\r\n"
        + "        UVScale: {X: 2.5, Y: 2.5}\r\n"
        + "Layers: {}\r\n";
}
