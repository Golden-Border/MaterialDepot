using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Stride;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Tests.Settings;

public sealed class JsonSettingsStoreTests : IDisposable
{
    private static readonly DepotSettings defaults = new("C:/Library", "C:/Downloads", []);

    private readonly TempDirectory temp = new();
    private readonly JsonSettingsStore sut;

    public JsonSettingsStoreTests()
    {
        sut = new JsonSettingsStore(temp.Combine("MaterialDepot", "settings.json"), () => defaults);
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void Load_WithoutFile_GivesTheDefaults()
    {
        // Act & Assert
        sut.Load().ShouldBe(defaults);
    }

    [Fact]
    public void Load_AfterSave_GivesTheSavedSettings()
    {
        // Arrange
        var project = new StrideProject("Game", "C:/repos/Game", "Assets/Textures", "Game/Assets", "Materials", "Materials");
        var settings = new DepotSettings("D:/Textures", "D:/Downloads", [project]);

        // Act
        sut.Save(settings);
        var loaded = sut.Load();

        // Assert
        loaded.LibraryRoot.ShouldBe("D:/Textures");
        loaded.DownloadsDirectory.ShouldBe("D:/Downloads");
        loaded.Projects.ShouldHaveSingleItem().ShouldBe(project);
    }

    [Fact]
    public void Load_SettingsFromBeforeThePatternsExisted_GivesTheDefaultPatternsAndMapNames()
    {
        // Arrange
        Directory.CreateDirectory(temp.Combine("MaterialDepot"));
        File.WriteAllText(temp.Combine("MaterialDepot", "settings.json"), """{ "LibraryRoot": "D:/Textures", "DownloadsDirectory": "D:/Downloads", "Projects": [] }""");

        // Act
        var loaded = sut.Load();

        // Assert
        loaded.LibraryRoot.ShouldBe("D:/Textures");
        loaded.FilePatterns.ShouldBe(DefaultTextureNaming.FilePatterns);
        loaded.MapNames.ShouldBe(DefaultTextureNaming.MapNames);
    }

    [Fact]
    public void Load_AfterSave_GivesTheSavedPatternsAndMapNames()
    {
        // Arrange
        var mapNames = new Dictionary<PbrMap, IReadOnlyList<string>> { [PbrMap.Albedo] = ["col", "base"], [PbrMap.Normal] = ["nrm"] };
        var settings = new DepotSettings("D:/Textures", "D:/Downloads", [], [new FilePattern("Test", @"^(?<name>.+)\.(?<map>[a-z]+)$")], mapNames);

        // Act
        sut.Save(settings);
        var loaded = sut.Load();

        // Assert
        loaded.FilePatterns.ShouldBe([new FilePattern("Test", @"^(?<name>.+)\.(?<map>[a-z]+)$")]);
        loaded.MapNames[PbrMap.Albedo].ShouldBe(["col", "base"]);
        loaded.MapNames[PbrMap.Normal].ShouldBe(["nrm"]);
    }

    [Fact]
    public void CreateParser_WithDefaultSettings_RecognisesTexturesComAndPolyHavenNames()
    {
        // Arrange
        var parser = new DepotSettings("D:/Textures", "D:/Downloads", []).CreateParser();

        // Act & Assert
        parser.TryParse("TCom_HeavyRust3_1K_albedo.tif", out var texturesCom).ShouldBeTrue();
        texturesCom.BaseName.ShouldBe("TCom_HeavyRust3_1K");
        parser.TryParse("aerial_rocks_04_nor_gl_4k.png", out var polyHaven).ShouldBeTrue();
        polyHaven.Map.ShouldBe(PbrMap.Normal);
    }

    [Fact]
    public void Load_BrokenFile_GivesTheDefaults()
    {
        // Arrange
        Directory.CreateDirectory(temp.Combine("MaterialDepot"));
        File.WriteAllText(temp.Combine("MaterialDepot", "settings.json"), "{ not json");

        // Act & Assert
        sut.Load().ShouldBe(defaults);
    }

    [Fact]
    public void CreateDefault_PointsAtLibraryAndDownloadsWithoutProjects()
    {
        // Act
        var settings = DepotSettings.CreateDefault(temp.Path);

        // Assert
        settings.LibraryRoot.ShouldBe(temp.Combine("Textures"));
        settings.DownloadsDirectory.ShouldBe(temp.Combine("Downloads"));
        settings.Projects.ShouldBeEmpty();
    }
}
