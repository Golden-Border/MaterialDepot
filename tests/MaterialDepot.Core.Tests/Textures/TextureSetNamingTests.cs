using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Textures;

public class TextureSetNamingTests
{
    [Theory]
    [InlineData("TCom_HeavyRust3_1K", "Heavy Rust 3")]
    [InlineData("TCom_Metal_GoldOld_1K", "Gold Old")]
    [InlineData("TCom_Rock_CliffVolcanic_2K", "Cliff Volcanic")]
    [InlineData("TCom_AncientFloorB_2K", "Ancient Floor B")]
    [InlineData("TCom_Wood_PlanksOld3_3.5x3.5_2K", "Planks Old 3")]
    [InlineData("TexturesCom_Ground_FarmlandDry_4.7x4.7_1K", "Farmland Dry")]
    [InlineData("aerial_rocks_04", "Aerial Rocks 04")]
    [InlineData("TCom_1K", "TCom_1K")]
    public void SuggestName_MakesAReadableName(string baseName, string expected)
    {
        // Act & Assert
        TextureSetNaming.SuggestName(baseName).ShouldBe(expected);
    }

    [Theory]
    [InlineData("TCom_Ground_GravelBlack_4x4_2K", 4.0)]
    [InlineData("TCom_Wood_PlanksOld3_3.5x3.5_2K", 3.5)]
    [InlineData("TexturesCom_Ground_FarmlandDry_4.7x4.7_1K", 4.7)]
    public void TileSizeMeters_ReadsTheSizeToken(string baseName, double expected)
    {
        // Act & Assert
        TextureSetNaming.TileSizeMeters(baseName).ShouldBe(expected);
    }

    [Fact]
    public void TileSizeMeters_WithoutSizeToken_IsNull()
    {
        // Act & Assert
        TextureSetNaming.TileSizeMeters("TCom_HeavyRust3_1K").ShouldBeNull();
    }
}
