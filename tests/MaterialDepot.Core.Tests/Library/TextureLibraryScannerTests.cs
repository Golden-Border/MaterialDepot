using MaterialDepot.Core.Library;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Library;

public sealed class TextureLibraryScannerTests : IDisposable
{
    private readonly TempDirectory library = new();
    private readonly TextureLibraryScanner sut = new();

    public void Dispose() => library.Dispose();

    [Fact]
    public void Scan_WithCustomPatterns_FindsTheMapsTheyDescribe()
    {
        // Arrange
        var parser = new MapFileParser([@"^(?<map>[a-z]+)-(?<name>.+)$"], DefaultTextureNaming.MapNames);
        Touch(string.Empty, "albedo-Brick.png", "normal-Brick.png", "Brick_albedo.png");

        // Act
        var sets = sut.Scan(library.Path, parser);

        // Assert
        var set = sets.ShouldHaveSingleItem();
        set.Name.ShouldBe("Brick");
        set.Maps.Keys.ShouldBe([PbrMap.Albedo, PbrMap.Normal], ignoreOrder: true);
    }

    [Fact]
    public void Scan_FolderWithOneSet_NamesTheSetAfterTheFolder()
    {
        // Arrange
        Touch("Heavy Rust 3", "TCom_HeavyRust3_1K_albedo.tif", "TCom_HeavyRust3_1K_normal.tif", "TCom_HeavyRust3_1K_height.tif");

        // Act
        var sets = sut.Scan(library.Path, MapFileParser.Default);

        // Assert
        var set = sets.ShouldHaveSingleItem();
        set.Name.ShouldBe("Heavy Rust 3");
        set.Maps.Keys.ShouldBe([PbrMap.Albedo, PbrMap.Normal, PbrMap.Height], ignoreOrder: true);
        set.Albedo!.Stem.ShouldBe("TCom_HeavyRust3_1K_albedo");
    }

    [Fact]
    public void Scan_FolderWhoseMapsHaveDifferentBaseNames_IsStillOneSet()
    {
        // Arrange
        Touch("Anodized Aluminium", "TCom_AnodizedAluminium_1K_albedo.tif", "TCom_AnodizedAluminium_New_1K_metallic.tif", "TCom_AnodizedAluminium_New_1K_normal.tif");

        // Act
        var sets = sut.Scan(library.Path, MapFileParser.Default);

        // Assert
        sets.ShouldHaveSingleItem().Maps.Count.ShouldBe(3);
    }

    [Fact]
    public void Scan_FolderWithTwoSets_GivesOneSetPerBaseName()
    {
        // Arrange
        Touch("Cliff Rock", "TCom_Rock_Cliff_A_2x2_2K_albedo.tif", "TCom_Rock_Cliff_A_2x2_2K_normal.tif", "TCom_Rock_Cliff_B_2x2_2K_albedo.tif");

        // Act
        var sets = sut.Scan(library.Path, MapFileParser.Default);

        // Assert
        sets.Select(set => set.Name).ShouldBe(["Cliff Rock (TCom_Rock_Cliff_A_2x2_2K)", "Cliff Rock (TCom_Rock_Cliff_B_2x2_2K)"]);
    }

    [Fact]
    public void Scan_SameMapAsTiffAndPng_PrefersTheTiff()
    {
        // Arrange
        Touch("Earth Rocks", "TCom_Ground_EarthRocks_2x2_2K_albedo.png", "TCom_Ground_EarthRocks_2x2_2K_albedo.tif");

        // Act
        var set = sut.Scan(library.Path, MapFileParser.Default).ShouldHaveSingleItem();

        // Assert
        set.Albedo!.Path.ShouldEndWith(".tif");
    }

    [Fact]
    public void Scan_FindsNestedSetsAndLooseRootFilesAndSkipsTheRest()
    {
        // Arrange
        Touch(Path.Combine("Floor", "Ancient Stone Floor"), "TCom_AncientFloorB_2K_albedo.tif");
        Touch("", "TexturesCom_Pavement_CobblestoneMedieval12_4x4_1K_albedo.png", "TexturesCom_Skies0186_S.jpg", "Camera.xcf");
        Touch("Particles", "spark.png");

        // Act
        var sets = sut.Scan(library.Path, MapFileParser.Default);

        // Assert
        sets.Select(set => set.Name).ShouldBe(["Ancient Stone Floor", "TexturesCom_Pavement_CobblestoneMedieval12_4x4_1K"]);
    }

    [Fact]
    public void Scan_MissingRoot_IsEmpty()
    {
        // Act & Assert
        sut.Scan(library.Combine("missing"), MapFileParser.Default).ShouldBeEmpty();
    }

    private void Touch(string folder, params string[] fileNames)
    {
        var directory = library.Combine(folder);
        Directory.CreateDirectory(directory);
        foreach (var fileName in fileNames)
        {
            File.WriteAllBytes(Path.Combine(directory, fileName), []);
        }
    }
}
