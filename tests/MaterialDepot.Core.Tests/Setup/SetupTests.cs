using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Setup;
using MaterialDepot.Core.Stride;
using MaterialDepot.Core.Textures;
using Moq;

namespace MaterialDepot.Core.Tests.Setup;

public sealed class SetupTests : IDisposable
{
    private readonly TempDirectory temp = new();
    private readonly Mock<ISettingsStore> settingsStore = new();
    private readonly SetupGuide sut;

    public SetupTests()
    {
        sut = new SetupGuide(settingsStore.Object);
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void IsFirstRun_NothingSavedYet_IsTrue()
    {
        // Arrange
        settingsStore.SetupGet(store => store.HasSavedSettings).Returns(false);

        // Act & Assert
        sut.IsFirstRun.ShouldBeTrue();
    }

    [Fact]
    public void IsFirstRun_SettingsSaved_IsFalse()
    {
        // Arrange
        settingsStore.SetupGet(store => store.HasSavedSettings).Returns(true);

        // Act & Assert
        sut.IsFirstRun.ShouldBeFalse();
    }

    [Fact]
    public void HasSavedSettings_FollowsTheSettingsFile()
    {
        // Arrange
        var store = new JsonSettingsStore(temp.Combine("settings.json"), () => DepotSettings.CreateDefault(temp.Path));
        var before = store.HasSavedSettings;

        // Act
        store.Save(store.Load());

        // Assert
        before.ShouldBeFalse();
        store.HasSavedSettings.ShouldBeTrue();
    }

    [Fact]
    public void Apply_TakesTheFoldersAndCreatesTheLibrary()
    {
        // Arrange
        var current = DepotSettings.CreateDefault(temp.Path);
        var library = temp.Combine("Meine Bibliothek");

        // Act
        var settings = sut.Apply(current, "  " + library + " ", temp.Combine("Eingang"), null);

        // Assert
        settings.LibraryRoot.ShouldBe(library);
        settings.DownloadsDirectory.ShouldBe(temp.Combine("Eingang"));
        settings.Projects.ShouldBeEmpty();
        Directory.Exists(library).ShouldBeTrue();
    }

    [Fact]
    public void Apply_WithProject_AddsItNextToTheOthers()
    {
        // Arrange
        var existing = new StrideProject("Alt", temp.Combine("Alt"), "Assets/Textures", "Assets", "Textures", "Materials");
        var current = new DepotSettings(temp.Combine("Lib"), temp.Combine("Down"), [existing]);
        var added = SetupSuggestions.ForProject(temp.Combine("Neu"));

        // Act
        var settings = sut.Apply(current, current.LibraryRoot, current.DownloadsDirectory, added);

        // Assert
        settings.Projects.ShouldBe([existing, added]);
    }

    [Fact]
    public void Apply_KeepsTheFileNamesAndTheLanguage()
    {
        // Arrange
        var patterns = new[] { new FilePattern("Eigenes", @"^(?<name>.+)\.(?<map>[a-z]+)$") };
        var mapNames = new Dictionary<PbrMap, IReadOnlyList<string>> { [PbrMap.Albedo] = ["col"] };
        var current = new DepotSettings(temp.Combine("Lib"), temp.Combine("Down"), [], patterns, mapNames, "en");

        // Act
        var settings = sut.Apply(current, temp.Combine("Neu"), current.DownloadsDirectory, null);

        // Assert
        settings.FilePatterns.ShouldBe(patterns);
        settings.MapNames.ShouldBeSameAs(mapNames);
        settings.Language.ShouldBe("en");
    }

    [Fact]
    public void Apply_ProjectInTheSameFolder_ReplacesTheOldEntry()
    {
        // Arrange
        var existing = new StrideProject("Alt", temp.Combine("Spiel"), "Assets/Textures", "Assets", "Textures", "Materials");
        var current = new DepotSettings(temp.Combine("Lib"), temp.Combine("Down"), [existing]);
        var replacement = new StrideProject("Neu", temp.Combine("Spiel") + Path.DirectorySeparatorChar, "Assets/Textures", "Assets", "Tex", "Mat");

        // Act
        var settings = sut.Apply(current, current.LibraryRoot, current.DownloadsDirectory, replacement);

        // Assert
        settings.Projects.ShouldHaveSingleItem().Name.ShouldBe("Neu");
    }

    [Fact]
    public void CheckFolder_ClassifiesThePath()
    {
        // Arrange
        var file = temp.Combine("datei.txt");
        File.WriteAllText(file, "x");

        // Act & Assert
        SetupValidator.CheckFolder(temp.Path).ShouldBe(FolderStatus.Exists);
        SetupValidator.CheckFolder(temp.Combine("fehlt")).ShouldBe(FolderStatus.Missing);
        SetupValidator.CheckFolder(file).ShouldBe(FolderStatus.IsFile);
        SetupValidator.CheckFolder("  ").ShouldBe(FolderStatus.Empty);
        SetupValidator.CheckFolder(null).ShouldBe(FolderStatus.Empty);
        SetupValidator.CheckFolder("Textures/relativ").ShouldBe(FolderStatus.Invalid);
    }

    [Fact]
    public void CheckProject_SuggestedProjectInExistingFolder_HasNoProblems()
    {
        // Arrange
        var project = SetupSuggestions.ForProject(temp.Path);

        // Act & Assert
        SetupValidator.CheckProject(project).ShouldBeEmpty();
    }

    [Fact]
    public void CheckProject_ReportsEveryProblem()
    {
        // Arrange
        var project = new StrideProject(" ", temp.Combine("fehlt"), "", "C:/Assets", "../Textures", "a/../../b");

        // Act
        var problems = SetupValidator.CheckProject(project);

        // Assert
        problems.ShouldBe(
        [
            ProjectProblem.NameMissing,
            ProjectProblem.RootMissing,
            ProjectProblem.SourceTextureDirectoryInvalid,
            ProjectProblem.AssetsDirectoryInvalid,
            ProjectProblem.TextureAssetDirectoryInvalid,
            ProjectProblem.MaterialDirectoryInvalid,
        ]);
    }

    [Fact]
    public void ForProject_WithoutPackage_UsesTheDefaults()
    {
        // Arrange
        var root = temp.Combine("Spiel");
        Directory.CreateDirectory(root);

        // Act
        var project = SetupSuggestions.ForProject(root + Path.DirectorySeparatorChar);

        // Assert
        project.ShouldBe(new StrideProject("Spiel", root, "Assets/Textures", "Assets", "Textures", "Materials"));
    }

    [Fact]
    public void ForProject_WithPackageBelowTheRoot_PutsTheAssetsNextToIt()
    {
        // Arrange
        var root = temp.Combine("Spiel");
        Directory.CreateDirectory(Path.Combine(root, "src", "Game"));
        File.WriteAllText(Path.Combine(root, "src", "Game", "Game.sdpkg"), string.Empty);

        // Act
        var project = SetupSuggestions.ForProject(root);

        // Assert
        project.AssetsDirectory.ShouldBe("src/Game/Assets");
        project.SourceTextureDirectory.ShouldBe("Assets/Textures");
    }

    [Fact]
    public void ForProject_WithPackageInTheRoot_UsesTheAssetsFolderThere()
    {
        // Arrange
        var root = temp.Combine("Spiel");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "Spiel.sdpkg"), string.Empty);

        // Act
        var project = SetupSuggestions.ForProject(root);

        // Assert
        project.AssetsDirectory.ShouldBe("Assets");
    }
}
