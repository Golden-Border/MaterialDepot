using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Stride;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The settings tab: library and download folder, how texture files are named, and the Stride projects to import into.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly DepotSession session;
    private readonly IDialogService dialogs;

    public SettingsViewModel(DepotSession session, IDialogService dialogs)
    {
        this.session = session;
        this.dialogs = dialogs;
        Patterns.CollectionChanged += (_, _) => UpdateTestResult();
        Reset();
    }

    public ObservableCollection<ProjectEditorViewModel> Projects { get; } = [];

    public ObservableCollection<PatternEditorViewModel> Patterns { get; } = [];

    public ObservableCollection<MapNamesEditorViewModel> MapNames { get; } = [];

    [ObservableProperty]
    public partial string LibraryRoot { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DownloadsDirectory { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveProjectCommand))]
    public partial ProjectEditorViewModel? SelectedProject { get; set; }

    /// <summary>
    /// A file name to try the patterns on without saving them.
    /// </summary>
    [ObservableProperty]
    public partial string TestFileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TestResult { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SavedNote { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorNote { get; set; } = string.Empty;

    [RelayCommand]
    private void BrowseLibrary() => LibraryRoot = dialogs.PickFolder(LibraryRoot) ?? LibraryRoot;

    [RelayCommand]
    private void BrowseDownloads() => DownloadsDirectory = dialogs.PickFolder(DownloadsDirectory) ?? DownloadsDirectory;

    [RelayCommand]
    private void BrowseProjectRoot(ProjectEditorViewModel project) => project.RootDirectory = dialogs.PickFolder(project.RootDirectory) ?? project.RootDirectory;

    [RelayCommand]
    private void AddProject()
    {
        var project = new ProjectEditorViewModel(new StrideProject("Neues Projekt", string.Empty, "Assets/Textures", "Assets", "Materials", "Materials"));
        Projects.Add(project);
        SelectedProject = project;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProject))]
    private void RemoveProject()
    {
        if (SelectedProject is { } project && dialogs.Confirm($"Projekt „{project.Name}“ aus der Liste nehmen? Am Projekt selbst ändert sich nichts."))
        {
            Projects.Remove(project);
        }
    }

    [RelayCommand]
    private void AddPattern() => Patterns.Add(Watch(new PatternEditorViewModel(new FilePattern("Neues Muster", "^(?<name>.+?)_(?<map>[A-Za-z]+)$"))));

    [RelayCommand]
    private void RemovePattern(PatternEditorViewModel pattern) => Patterns.Remove(pattern);

    [RelayCommand]
    private void RestoreDefaultNaming()
    {
        ShowNaming(DefaultTextureNaming.FilePatterns, DefaultTextureNaming.MapNames);
        UpdateTestResult();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorNote = string.Empty;
        SavedNote = string.Empty;
        if (Patterns.Count == 0)
        {
            ErrorNote = "Mindestens ein Muster für Dateinamen ist nötig.";
            return;
        }

        if (Patterns.Any(pattern => pattern.HasError))
        {
            ErrorNote = "Ein Muster für Dateinamen ist ungültig. Bitte korrigieren, dann speichern.";
            return;
        }

        var old = session.Settings;
        var settings = new DepotSettings(
            LibraryRoot.Trim(),
            DownloadsDirectory.Trim(),
            [.. Projects.Select(project => project.ToProject())],
            [.. Patterns.Select(pattern => pattern.ToPattern())],
            MapNames.ToDictionary(row => row.Map, row => row.Names));
        var rescan = old.LibraryRoot != settings.LibraryRoot || !SameNaming(old, settings);
        session.SaveSettings(settings);
        SavedNote = "Gespeichert.";
        if (rescan)
        {
            await session.RescanLibraryAsync();
        }
    }

    [RelayCommand]
    private void Reset()
    {
        var settings = session.Settings;
        LibraryRoot = settings.LibraryRoot;
        DownloadsDirectory = settings.DownloadsDirectory;
        Projects.Clear();
        foreach (var project in settings.Projects)
        {
            Projects.Add(new ProjectEditorViewModel(project));
        }

        SelectedProject = Projects.FirstOrDefault();
        ShowNaming(settings.FilePatterns, settings.MapNames);
        UpdateTestResult();
        SavedNote = string.Empty;
        ErrorNote = string.Empty;
    }

    partial void OnTestFileNameChanged(string value) => UpdateTestResult();

    private static bool SameNaming(DepotSettings a, DepotSettings b) =>
        a.FilePatterns.SequenceEqual(b.FilePatterns)
        && a.MapNames.Count == b.MapNames.Count
        && a.MapNames.All(pair => b.MapNames.TryGetValue(pair.Key, out var names) && pair.Value.SequenceEqual(names));

    private void ShowNaming(IEnumerable<FilePattern> patterns, IReadOnlyDictionary<PbrMap, IReadOnlyList<string>> mapNames)
    {
        Patterns.Clear();
        foreach (var pattern in patterns)
        {
            Patterns.Add(Watch(new PatternEditorViewModel(pattern)));
        }

        MapNames.Clear();
        foreach (var map in Enum.GetValues<PbrMap>())
        {
            MapNames.Add(Watch(new MapNamesEditorViewModel(map, mapNames.GetValueOrDefault(map) ?? [])));
        }
    }

    private T Watch<T>(T editor)
        where T : INotifyPropertyChanged
    {
        editor.PropertyChanged += (_, _) => UpdateTestResult();
        return editor;
    }

    /// <summary>
    /// What the patterns as edited, saved or not, make of <see cref="TestFileName"/>.
    /// </summary>
    private void UpdateTestResult()
    {
        var fileName = TestFileName.Trim();
        if (fileName.Length == 0)
        {
            TestResult = string.Empty;
            return;
        }

        var parser = new MapFileParser(Patterns.Select(pattern => pattern.ToPattern()), MapNames.ToDictionary(row => row.Map, row => row.Names));
        var parsed = parser.Parse(Path.GetFileNameWithoutExtension(fileName));
        TestResult = parsed switch
        {
            null => "Kein Muster passt auf diesen Dateinamen.",
            { Map: null } => $"Die Map-Bezeichnung „{parsed.MapToken}“ ist bei keiner Map eingetragen.",
            _ when !parser.TryParse(fileName, out _) => "Das ist keine Bilddatei (tif, png, jpg).",
            _ => $"Set: {parsed.Name} · Map: {MapLabels.Of(parsed.Map.Value)} · Auflösung: {parsed.Resolution ?? "–"}",
        };
    }

    private bool HasSelectedProject() => SelectedProject is not null;
}
