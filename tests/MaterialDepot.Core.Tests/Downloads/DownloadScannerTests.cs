using System.IO.Compression;
using MaterialDepot.Core.Downloads;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Downloads;

public sealed class DownloadScannerTests : IDisposable
{
    private readonly TempDirectory temp = new();
    private readonly string downloads;
    private readonly DownloadScanner sut = new();

    public DownloadScannerTests()
    {
        downloads = temp.Combine("Downloads");
        Directory.CreateDirectory(downloads);
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void Scan_GroupsLooseMapFilesByBaseName()
    {
        // Arrange
        Write("TCom_HeavyRust3_1K_albedo.tif", "TCom_HeavyRust3_1K_normal.tif", "TCom_Rock_CliffVolcanic_2K_albedo.tif", "TCom_Rock_CliffVolcanic_2K_ao.tif", "invoice.pdf", "photo.jpg");

        // Act
        var candidates = sut.Scan(downloads, []);

        // Assert
        candidates.Select(candidate => candidate.BaseName).ShouldBe(["TCom_HeavyRust3_1K", "TCom_Rock_CliffVolcanic_2K"]);
        var heavyRust = candidates[0];
        heavyRust.SuggestedName.ShouldBe("Heavy Rust 3");
        heavyRust.Maps.ShouldBe([PbrMap.Albedo, PbrMap.Normal]);
        heavyRust.IsInLibrary.ShouldBeFalse();
    }

    [Fact]
    public void Scan_FindsMapsInsideAZip()
    {
        // Arrange
        var zipPath = Path.Combine(downloads, "PBR00913.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            zip.CreateEntry("Heavy Rust 3/TCom_HeavyRust3_1K_albedo.tif");
            zip.CreateEntry("Heavy Rust 3/TCom_HeavyRust3_1K_ao.tif");
            zip.CreateEntry("Heavy Rust 3/readme.txt");
        }

        // Act
        var candidate = sut.Scan(downloads, []).ShouldHaveSingleItem();

        // Assert
        candidate.Files.Select(file => file.ZipEntry).ShouldBe(["Heavy Rust 3/TCom_HeavyRust3_1K_albedo.tif", "Heavy Rust 3/TCom_HeavyRust3_1K_ao.tif"]);
        candidate.SourceFiles.ShouldBe([zipPath]);
    }

    [Fact]
    public void Scan_IgnoresALoneMap()
    {
        // Arrange
        Write("snapshot-for-split-button-basic-1-color.png", "TCom_HeavyRust3_1K_albedo.tif");

        // Act & Assert
        sut.Scan(downloads, []).ShouldBeEmpty();
    }

    [Fact]
    public void Scan_IgnoresABrokenZip()
    {
        // Arrange
        File.WriteAllText(Path.Combine(downloads, "half.zip"), "not a zip");

        // Act & Assert
        sut.Scan(downloads, []).ShouldBeEmpty();
    }

    [Fact]
    public void Scan_MarksACandidateWhoseFilesAreInALibrarySet()
    {
        // Arrange
        Write("TCom_HeavyRust3_1K_albedo.tif", "TCom_HeavyRust3_1K_normal.tif");
        var setDirectory = temp.Combine("Library", "Heavy Rust 3");
        Directory.CreateDirectory(setDirectory);
        File.WriteAllBytes(Path.Combine(setDirectory, "TCom_HeavyRust3_1K_albedo.tif"), []);
        File.WriteAllBytes(Path.Combine(setDirectory, "TCom_HeavyRust3_1K_normal.tif"), []);
        var library = new[] { new TextureSet("Heavy Rust 3", setDirectory, new Dictionary<PbrMap, MapFile>()) };

        // Act
        var candidate = sut.Scan(downloads, library).ShouldHaveSingleItem();

        // Assert
        candidate.LibrarySetName.ShouldBe("Heavy Rust 3");
    }

    [Fact]
    public void Scan_WithOnlySomeFilesInTheLibrary_IsNotMarked()
    {
        // Arrange
        Write("TCom_HeavyRust3_1K_albedo.tif", "TCom_HeavyRust3_1K_normal.tif");
        var setDirectory = temp.Combine("Library", "Heavy Rust 3");
        Directory.CreateDirectory(setDirectory);
        File.WriteAllBytes(Path.Combine(setDirectory, "TCom_HeavyRust3_1K_albedo.tif"), []);
        var library = new[] { new TextureSet("Heavy Rust 3", setDirectory, new Dictionary<PbrMap, MapFile>()) };

        // Act
        var candidate = sut.Scan(downloads, library).ShouldHaveSingleItem();

        // Assert
        candidate.IsInLibrary.ShouldBeFalse();
    }

    private void Write(params string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            File.WriteAllBytes(Path.Combine(downloads, fileName), [1, 2, 3]);
        }
    }
}
