using System.IO.Compression;
using MaterialDepot.Core.Downloads;
using MaterialDepot.Core.Textures;
using Moq;

namespace MaterialDepot.Core.Tests.Downloads;

public sealed class DownloadArchiverTests : IDisposable
{
    private readonly TempDirectory temp = new();
    private readonly string downloads;
    private readonly string library;
    private readonly Mock<IFileRemover> fileRemover = new();
    private readonly DownloadArchiver sut;

    public DownloadArchiverTests()
    {
        downloads = temp.Combine("Downloads");
        library = temp.Combine("Library");
        Directory.CreateDirectory(downloads);
        sut = new DownloadArchiver(fileRemover.Object);
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void Archive_CopiesLooseFilesUnchangedIntoTheSetFolder()
    {
        // Arrange
        var candidate = Candidate(Loose("TCom_HeavyRust3_1K_albedo.tif", [1, 2, 3]), Loose("TCom_HeavyRust3_1K_height.tif", [4, 5]));

        // Act
        var result = sut.Archive(candidate, "Heavy Rust 3", library, move: false);

        // Assert
        result.SetDirectory.ShouldBe(Path.Combine(library, "Heavy Rust 3"));
        result.Copied.Count.ShouldBe(2);
        File.ReadAllBytes(Path.Combine(library, "Heavy Rust 3", "TCom_HeavyRust3_1K_albedo.tif")).ShouldBe([1, 2, 3]);
        File.Exists(Path.Combine(downloads, "TCom_HeavyRust3_1K_albedo.tif")).ShouldBeTrue();
        fileRemover.Verify(remover => remover.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Archive_UnpacksMapsFromAZip()
    {
        // Arrange
        var zipPath = Path.Combine(downloads, "PBR00913.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            using var stream = zip.CreateEntry("maps/TCom_HeavyRust3_1K_ao.tif").Open();
            stream.Write([7, 8, 9]);
        }

        var candidate = Candidate(new DownloadedMap("TCom_HeavyRust3_1K_ao.tif", PbrMap.AmbientOcclusion, zipPath, "maps/TCom_HeavyRust3_1K_ao.tif", 3));

        // Act
        sut.Archive(candidate, "Heavy Rust 3", library, move: false);

        // Assert
        File.ReadAllBytes(Path.Combine(library, "Heavy Rust 3", "TCom_HeavyRust3_1K_ao.tif")).ShouldBe([7, 8, 9]);
    }

    [Fact]
    public void Archive_WithMove_RemovesTheDownloads()
    {
        // Arrange
        var albedo = Loose("TCom_HeavyRust3_1K_albedo.tif", [1]);
        var candidate = Candidate(albedo);

        // Act
        var result = sut.Archive(candidate, "Heavy Rust 3", library, move: true);

        // Assert
        result.Removed.ShouldBe([albedo.Location]);
        fileRemover.Verify(remover => remover.Remove(albedo.Location), Times.Once);
    }

    [Fact]
    public void Archive_SameFileAlreadyThere_LeavesItAndStillMoves()
    {
        // Arrange
        var albedo = Loose("TCom_HeavyRust3_1K_albedo.tif", [1, 2, 3]);
        Directory.CreateDirectory(Path.Combine(library, "Heavy Rust 3"));
        File.WriteAllBytes(Path.Combine(library, "Heavy Rust 3", albedo.FileName), [1, 2, 3]);

        // Act
        var result = sut.Archive(Candidate(albedo), "Heavy Rust 3", library, move: true);

        // Assert
        result.Copied.ShouldBeEmpty();
        result.AlreadyThere.ShouldHaveSingleItem();
        result.Removed.ShouldHaveSingleItem();
    }

    [Fact]
    public void Archive_DifferentFileOfTheSameName_IsNeverOverwrittenAndBlocksTheMove()
    {
        // Arrange
        var albedo = Loose("TCom_HeavyRust3_1K_albedo.tif", [1, 2, 3]);
        var existing = Path.Combine(library, "Heavy Rust 3", albedo.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(existing)!);
        File.WriteAllBytes(existing, [9, 9, 9]);

        // Act
        var result = sut.Archive(Candidate(albedo), "Heavy Rust 3", library, move: true);

        // Assert
        result.Conflicts.ShouldBe([existing]);
        File.ReadAllBytes(existing).ShouldBe([9, 9, 9]);
        result.Removed.ShouldBeEmpty();
        fileRemover.Verify(remover => remover.Remove(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Rust: Heavy")]
    public void Archive_WithAnInvalidSetName_Throws(string setName)
    {
        // Arrange
        var candidate = Candidate(Loose("TCom_HeavyRust3_1K_albedo.tif", [1]));

        // Act & Assert
        Should.Throw<ArgumentException>(() => sut.Archive(candidate, setName, library, move: false));
    }

    private DownloadedMap Loose(string fileName, byte[] content)
    {
        var path = Path.Combine(downloads, fileName);
        File.WriteAllBytes(path, content);
        MapFile.TryParse(path, out var mapFile).ShouldBeTrue();
        return new DownloadedMap(fileName, mapFile.Map, path, null, content.Length);
    }

    private static DownloadCandidate Candidate(params DownloadedMap[] files) => new("TCom_HeavyRust3_1K", "Heavy Rust 3", files, null);
}
