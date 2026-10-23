using MaterialDepot.Core.Stride;

namespace MaterialDepot.Core.Tests.Stride;

public sealed class StrideProjectInventoryTests : IDisposable
{
    private readonly TempDirectory temp = new();
    private readonly StrideProject project;
    private readonly StrideProjectInventory sut = new();

    public StrideProjectInventoryTests()
    {
        project = new StrideProject("Probe", temp.Path, "Assets/Textures", "src/Game/Assets", "Materials", "Materials");
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void ImportedStems_CollectsTheSourceOfEveryTextureAsset()
    {
        // Arrange
        WriteTexture("Materials/Heavy Rust 3/TCom_HeavyRust3_1K_albedo.sdtex", "../../../../../Assets/Textures/Heavy Rust 3/TCom_HeavyRust3_1K_albedo.png");
        WriteTexture("Models/Torch/Torch_BaseColor.sdtex", "../../../../../Assets/Models/Torch/Torch_BaseColor.1001.png");

        // Act
        var stems = sut.ImportedStems(project);

        // Assert
        stems.ShouldBe(["TCom_HeavyRust3_1K_albedo", "Torch_BaseColor.1001"], ignoreOrder: true);
        stems.Contains("tcom_heavyrust3_1k_albedo").ShouldBeTrue();
    }

    [Fact]
    public void ImportedStems_WithoutAssetFolder_IsEmpty()
    {
        // Act & Assert
        sut.ImportedStems(project).ShouldBeEmpty();
    }

    private void WriteTexture(string relativePath, string source)
    {
        var path = Path.Combine(project.AssetsPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, $"!Texture\r\nId: {Guid.NewGuid()}\r\nTags: []\r\nSource: !file {source}\r\nType: !GrayscaleTextureType {{}}\r\n");
    }
}
