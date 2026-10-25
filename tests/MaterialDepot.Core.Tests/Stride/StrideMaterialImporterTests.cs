using ImageMagick;
using MaterialDepot.Core.Imaging;
using MaterialDepot.Core.Stride;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Stride;

public sealed class StrideMaterialImporterTests : IDisposable
{
    private readonly TempDirectory temp = new();
    private readonly StrideProject project;
    private readonly StrideMaterialImporter sut = new(new MagickImageConverter());

    public StrideMaterialImporterTests()
    {
        project = new StrideProject("Probe", temp.Combine("repo"), "Assets/Textures", "src/Game/Assets", "Materials", "Materials");
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void Import_WritesPngsTexturesAndMaterialWhereTheProjectKeepsThem()
    {
        // Arrange
        var set = Set(PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness, PbrMap.Metallic, PbrMap.AmbientOcclusion);

        // Act
        var result = sut.Import(Request(set));

        // Assert
        var textures = Path.Combine(project.RootDirectory, "Assets", "Textures", "Heavy Rust 3");
        var assets = Path.Combine(project.RootDirectory, "src", "Game", "Assets", "Materials");
        result.WrittenFiles.ShouldBe(
        [
            Path.Combine(textures, "TCom_HeavyRust3_1K_albedo.png"),
            Path.Combine(assets, "Heavy Rust 3", "TCom_HeavyRust3_1K_albedo.sdtex"),
            Path.Combine(textures, "TCom_HeavyRust3_1K_normal.png"),
            Path.Combine(assets, "Heavy Rust 3", "TCom_HeavyRust3_1K_normal.sdtex"),
            Path.Combine(textures, "TCom_HeavyRust3_1K_roughness.png"),
            Path.Combine(assets, "Heavy Rust 3", "TCom_HeavyRust3_1K_roughness.sdtex"),
            Path.Combine(textures, "TCom_HeavyRust3_1K_metallic.png"),
            Path.Combine(assets, "Heavy Rust 3", "TCom_HeavyRust3_1K_metallic.sdtex"),
            Path.Combine(textures, "TCom_HeavyRust3_1K_ao.png"),
            Path.Combine(assets, "Heavy Rust 3", "TCom_HeavyRust3_1K_ao.sdtex"),
            Path.Combine(assets, "Forged Iron.sdmat"),
        ]);
        result.WrittenFiles.ShouldAllBe(file => File.Exists(file));
        result.ReplacedMaterial.ShouldBeFalse();
    }

    [Fact]
    public void Import_PointsEachTextureAtItsPngAndTheMaterialAtEachTexture()
    {
        // Arrange
        var set = Set(PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness);

        // Act
        var result = sut.Import(Request(set));

        // Assert
        var albedo = File.ReadAllText(Path.Combine(project.TextureAssetPath("Heavy Rust 3"), "TCom_HeavyRust3_1K_albedo.sdtex"));
        albedo.ShouldContain("Source: !file ../../../../../Assets/Textures/Heavy Rust 3/TCom_HeavyRust3_1K_albedo.png\r\n");
        var albedoId = StrideAssetText.ReadId(albedo);
        File.ReadAllText(result.MaterialPath).ShouldContain($"Texture: {albedoId}:Materials/Heavy Rust 3/TCom_HeavyRust3_1K_albedo\r\n");
    }

    [Fact]
    public void Import_WritesSixteenBitSourcesAsEightBitPngs()
    {
        // Arrange
        var set = Set([PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness], depth: 16);

        // Act
        var result = sut.Import(Request(set));

        // Assert
        using var source = new MagickImage(set.Maps[PbrMap.Normal].Path);
        source.Depth.ShouldBe(16u);
        using var normal = new MagickImage(result.WrittenFiles[2]);
        normal.Format.ShouldBe(MagickFormat.Png);
        normal.Depth.ShouldBeLessThanOrEqualTo(8u);
    }

    [Fact]
    public void Import_Twice_WritesTheSameBytes()
    {
        // Arrange
        var set = Set(PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness, PbrMap.Metallic, PbrMap.AmbientOcclusion);
        var first = sut.Import(Request(set));
        var firstBytes = first.WrittenFiles.Select(File.ReadAllBytes).ToList();

        // Act
        var second = sut.Import(Request(set));

        // Assert
        second.WrittenFiles.ShouldBe(first.WrittenFiles);
        second.WrittenFiles.Select(File.ReadAllBytes).ToList().ShouldBe(firstBytes);
        second.ReplacedMaterial.ShouldBeTrue();
    }

    [Fact]
    public void Import_OverAnExistingMaterial_KeepsItsId()
    {
        // Arrange
        var existingId = Guid.Parse("8e7a48ea-d5dd-4229-9f8a-40cee92cef84");
        var materialPath = project.MaterialPath("Materials", "Forged Iron");
        Directory.CreateDirectory(Path.GetDirectoryName(materialPath)!);
        File.WriteAllText(materialPath, $"!MaterialAsset\r\nId: {existingId}\r\nTags: []\r\n");

        // Act
        var result = sut.Import(Request(Set(PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness)));

        // Assert
        result.MaterialId.ShouldBe(existingId);
        result.ReplacedMaterial.ShouldBeTrue();
        StrideAssetText.ReadId(File.ReadAllText(materialPath)).ShouldBe(existingId);
    }

    [Fact]
    public void Import_WithAGivenMaterialId_UsesIt()
    {
        // Arrange
        var givenId = Guid.NewGuid();

        // Act
        var result = sut.Import(Request(Set(PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness)) with { MaterialId = givenId });

        // Assert
        result.MaterialId.ShouldBe(givenId);
    }

    [Fact]
    public void Import_IgnoresTheHeightMap()
    {
        // Arrange
        var set = Set(PbrMap.Albedo, PbrMap.Normal, PbrMap.Roughness, PbrMap.Height);

        // Act
        var result = sut.Import(Request(set));

        // Assert
        result.WrittenFiles.ShouldNotContain(file => file.Contains("height"));
    }

    [Fact]
    public void Import_WithoutANormalMap_Throws()
    {
        // Arrange
        var set = Set(PbrMap.Albedo, PbrMap.Roughness);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => sut.Import(Request(set)));
        exception.Message.ShouldContain("Normal");
    }

    [Theory]
    [InlineData(0.2, 0.5, 2.5)]
    [InlineData(4.0, 1.0, 0.25)]
    [InlineData(1.0, 1.0, 1.0)]
    public void UvScaleFor_DividesTheMetersPerUvUnitByTheTileSize(double tileSize, double metersPerUvUnit, double expected)
    {
        // Act & Assert
        StrideImportRequest.UvScaleFor(tileSize, metersPerUvUnit).ShouldBe(expected, tolerance: 1e-9);
    }

    private StrideImportRequest Request(TextureSet set) => new(set, project, "Heavy Rust 3", "Forged Iron", "Materials", 2.5);

    private TextureSet Set(params PbrMap[] maps) => Set(maps, depth: 8);

    private TextureSet Set(PbrMap[] maps, uint depth)
    {
        var directory = temp.Combine("library", "Heavy Rust 3");
        var files = new Dictionary<PbrMap, MapFile>();
        foreach (var map in maps)
        {
            var token = map == PbrMap.AmbientOcclusion ? "ao" : map.ToString().ToLowerInvariant();
            var path = TestImages.WriteTiff(Path.Combine(directory, $"TCom_HeavyRust3_1K_{token}.tif"), depth: depth);
            MapFileParser.Default.TryParse(path, out var mapFile).ShouldBeTrue();
            files[map] = mapFile;
        }

        return new TextureSet("Heavy Rust 3", directory, files);
    }
}
