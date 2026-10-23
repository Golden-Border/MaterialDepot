using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Textures;

public class MapFileTests
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
    public void TryParse_RecognisesTheMapAndTheBaseName(string fileName, string baseName, PbrMap map)
    {
        // Act
        var recognised = MapFile.TryParse(Path.Combine("C:", "Textures", fileName), out var mapFile);

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
    public void TryParse_RejectsFilesThatAreNoMap(string fileName)
    {
        // Act
        var recognised = MapFile.TryParse(Path.Combine("C:", "Textures", fileName), out _);

        // Assert
        recognised.ShouldBeFalse();
    }
}
