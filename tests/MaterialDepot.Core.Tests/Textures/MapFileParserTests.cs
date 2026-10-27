using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Textures;

public class MapFileParserTests
{
    [Theory]
    [InlineData("TCom_HeavyRust3_1K_albedo.tif", "TCom_HeavyRust3_1K", PbrMap.Albedo)]
    [InlineData("TCom_HeavyRust3_1K_normal.tif", "TCom_HeavyRust3_1K", PbrMap.Normal)]
    [InlineData("TCom_HeavyRust3_1K_roughness.tif", "TCom_HeavyRust3_1K", PbrMap.Roughness)]
    [InlineData("TCom_HeavyRust3_1K_metallic.tif", "TCom_HeavyRust3_1K", PbrMap.Metallic)]
    [InlineData("TCom_HeavyRust3_1K_ao.tif", "TCom_HeavyRust3_1K", PbrMap.AmbientOcclusion)]
    [InlineData("TCom_AncientWall_2K_height.tif", "TCom_AncientWall_2K", PbrMap.Height)]
    [InlineData("TCom_Wood_PlanksOld3_3.5x3.5_2K_albedo.png", "TCom_Wood_PlanksOld3_3.5x3.5_2K", PbrMap.Albedo)]
    [InlineData("aerial_rocks_04_diff_4k.jpg", "aerial_rocks_04", PbrMap.Albedo)]
    [InlineData("aerial_rocks_04_nor_gl_4k.png", "aerial_rocks_04", PbrMap.Normal)]
    [InlineData("aerial_rocks_04_rough_4k.jpg", "aerial_rocks_04", PbrMap.Roughness)]
    [InlineData("TCom_Foo_1K_ALBEDO.TIF", "TCom_Foo_1K", PbrMap.Albedo)]
    [InlineData("Grass001_1K_Color.png", "Grass001_1K", PbrMap.Albedo)]
    [InlineData("Grass001_1K_NormalGL.png", "Grass001_1K", PbrMap.Normal)]
    [InlineData("Grass001_1K_AmbientOcclusion.png", "Grass001_1K", PbrMap.AmbientOcclusion)]
    [InlineData("Grass001_1K_Displacement.png", "Grass001_1K", PbrMap.Height)]
    [InlineData("Mossy Clif Rock 01_1K_Base Color.png", "Mossy Clif Rock 01_1K", PbrMap.Albedo)]
    [InlineData("Mossy Clif Rock 01_1K_Ambient Occlusion.png", "Mossy Clif Rock 01_1K", PbrMap.AmbientOcclusion)]
    [InlineData("Export_baseColor.tif", "Export", PbrMap.Albedo)]
    [InlineData("forest_floor-ao.png", "forest_floor", PbrMap.AmbientOcclusion)]
    [InlineData("forest_floor_Normal-ogl.png", "forest_floor", PbrMap.Normal)]
    public void TryParse_RecognisesTheMapAndTheBaseName(string fileName, string baseName, PbrMap map)
    {
        // Act
        var recognised = MapFileParser.Default.TryParse(Path.Combine("C:", "Textures", fileName), out var mapFile);

        // Assert
        recognised.ShouldBeTrue();
        mapFile.BaseName.ShouldBe(baseName);
        mapFile.Map.ShouldBe(map);
        mapFile.Stem.ShouldBe(Path.GetFileNameWithoutExtension(fileName));
    }

    [Theory]
    [InlineData("TexturesCom_FloorsMedieval0063_1_seamless_S.jpg")]
    [InlineData("Camera.xcf")]
    [InlineData("TCom_HeavyRust3_1K_albedo.zip")]
    [InlineData("_albedo.tif")]
    [InlineData("grassy_cobblestone_arm_4k.png")]
    [InlineData("Grass001_1K_NormalDX.png")]
    [InlineData("aerial_rocks_04_nor_dx_4k.png")]
    [InlineData("Grass 03_1K_Specular Level.png")]
    [InlineData("albedo.png")]
    public void TryParse_RejectsFilesThatAreNoMap(string fileName)
    {
        // Act
        var recognised = MapFileParser.Default.TryParse(Path.Combine("C:", "Textures", fileName), out _);

        // Assert
        recognised.ShouldBeFalse();
    }

    [Theory]
    [InlineData("aerial_rocks_04_nor_gl_4k", "aerial_rocks_04", "nor_gl", "4k")]
    [InlineData("rock_wall_diff_8k", "rock_wall", "diff", "8k")]
    [InlineData("Mossy Rock 01_Base Color", "Mossy Rock 01", "Base Color", null)]
    [InlineData("TCom_HeavyRust3_1K_albedo", "TCom_HeavyRust3_1K", "albedo", "1K")]
    public void Parse_GivesTheGroupsOfTheMatchingPattern(string stem, string name, string token, string? resolution)
    {
        // Act
        var parsed = MapFileParser.Default.Parse(stem);

        // Assert
        parsed.ShouldNotBeNull();
        parsed.Name.ShouldBe(name);
        parsed.MapToken.ShouldBe(token);
        parsed.Map.ShouldNotBeNull();
        parsed.Resolution.ShouldBe(resolution);
    }

    [Fact]
    public void Parse_UnknownMapName_GivesTheGroupsWithoutAMap()
    {
        // Act
        var parsed = MapFileParser.Default.Parse("Grass001_1K_NormalDX");

        // Assert
        parsed.ShouldNotBeNull();
        parsed.MapToken.ShouldBe("NormalDX");
        parsed.Map.ShouldBeNull();
    }

    [Fact]
    public void TryParse_UsesTheGivenPatternsAndMapNames()
    {
        // Arrange
        var sut = new MapFileParser(
            [new FilePattern("Test", @"^(?<map>[a-z]+)\.(?<name>.+)$")],
            new Dictionary<PbrMap, IReadOnlyList<string>> { [PbrMap.Albedo] = ["col"], [PbrMap.Normal] = ["nrm"] });

        // Act
        var recognised = sut.TryParse(Path.Combine("C:", "Textures", "col.Brick.png"), out var mapFile);

        // Assert
        recognised.ShouldBeTrue();
        mapFile.BaseName.ShouldBe("Brick");
        mapFile.Map.ShouldBe(PbrMap.Albedo);
        sut.TryParse(Path.Combine("C:", "Textures", "albedo.Brick.png"), out _).ShouldBeFalse();
    }

    [Fact]
    public void TryParse_SkipsInvalidPatternsInsteadOfThrowing()
    {
        // Arrange
        var sut = new MapFileParser([new FilePattern("Kaputt", "(?<name>"), new FilePattern("Ohne map", @"^(?<name>.+)_(?<other>x)$"), new FilePattern("Gut", @"^(?<name>.+)_(?<map>[a-z]+)$")], DefaultTextureNaming.MapNames);

        // Act
        var recognised = sut.TryParse(Path.Combine("C:", "Textures", "Brick_albedo.png"), out var mapFile);

        // Assert
        recognised.ShouldBeTrue();
        mapFile.BaseName.ShouldBe("Brick");
    }

    [Fact]
    public void TryParse_PatternThatBacktracksForever_IsNoMatchInsteadOfAHang()
    {
        // Arrange
        var sut = new MapFileParser([new FilePattern("Endlos", @"^(?<name>(a+)+)_(?<map>b)$")], DefaultTextureNaming.MapNames);

        // Act
        var recognised = sut.TryParse(Path.Combine("C:", "Textures", new string('a', 60) + "_c.png"), out _);

        // Assert
        recognised.ShouldBeFalse();
    }

    [Fact]
    public void TryParse_ImageExtensionIsStillRequired()
    {
        // Act
        var recognised = MapFileParser.Default.TryParse(Path.Combine("C:", "Textures", "Brick_albedo.zip"), out _);

        // Assert
        recognised.ShouldBeFalse();
    }

    [Theory]
    [InlineData(@"^(?<name>.+)_(?<map>[a-z]+)$")]
    [InlineData(@"^(?<name>.+)_(?<map>[a-z]+)_(?<res>\d+k)$")]
    public void Validate_PatternWithNameAndMap_IsFine(string pattern)
    {
        // Act & Assert
        MapFileParser.Validate(pattern).ShouldBeNull();
    }

    [Theory]
    [InlineData("", PatternProblem.Empty)]
    [InlineData("   ", PatternProblem.Empty)]
    [InlineData("(?<name>", PatternProblem.NotARegex)]
    [InlineData(@"^(?<name>.+)_(?<res>\d+k)$", PatternProblem.MapGroupMissing)]
    [InlineData(@"^(?<map>.+)$", PatternProblem.NameGroupMissing)]
    [InlineData(@"^(.+)_(.+)$", PatternProblem.NameAndMapGroupsMissing)]
    public void Validate_BadPattern_GivesTheProblem(string pattern, PatternProblem expected)
    {
        // Act & Assert
        MapFileParser.Validate(pattern).ShouldBe(expected);
    }
}
