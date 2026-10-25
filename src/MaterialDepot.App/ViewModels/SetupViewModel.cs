using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Setup;
using MaterialDepot.Core.Stride;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The setup guide: library, downloads, an optional Stride project, and how it all works.
/// </summary>
public sealed partial class SetupViewModel(DepotSession session, SetupGuide guide, IDialogService dialogs) : ObservableObject
{
    private const int lastStep = 3;

    [ObservableProperty]
    public partial int Step { get; set; }

    [ObservableProperty]
    public partial string LibraryRoot { get; set; } = session.Settings.LibraryRoot;

    [ObservableProperty]
    public partial string DownloadsDirectory { get; set; } = session.Settings.DownloadsDirectory;

    [ObservableProperty]
    public partial bool AddProject { get; set; }

    [ObservableProperty]
    public partial string ProjectName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProjectRootDirectory { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProjectSourceTextureDirectory { get; set; } = SetupSuggestions.SourceTextureDirectory;

    [ObservableProperty]
    public partial string ProjectAssetsDirectory { get; set; } = "Assets";

    [ObservableProperty]
    public partial string ProjectTextureAssetDirectory { get; set; } = SetupSuggestions.TextureAssetDirectory;

    [ObservableProperty]
    public partial string ProjectMaterialDirectory { get; set; } = SetupSuggestions.MaterialDirectory;

    /// <summary>
    /// Set to close the window: <see langword="true"/> when the settings were saved.
    /// </summary>
    [ObservableProperty]
    public partial bool? DialogResult { get; set; }

    public bool IsLibraryStep => Step == 0;

    public bool IsDownloadsStep => Step == 1;

    public bool IsProjectStep => Step == 2;

    public bool IsFlowStep => Step == lastStep;

    public bool IsLastStep => Step == lastStep;

    public bool IsNotFirstStep => Step > 0;

    public string StepTitle => Step switch
    {
        0 => "Schritt 1 von 4: Bibliothek",
        1 => "Schritt 2 von 4: Downloads",
        2 => "Schritt 3 von 4: Stride-Projekt (optional)",
        _ => "Schritt 4 von 4: So läuft es ab",
    };

    public string LibraryNote => FolderNote(SetupValidator.CheckFolder(LibraryRoot), missingIsFine: true);

    public string DownloadsNote => FolderNote(SetupValidator.CheckFolder(DownloadsDirectory), missingIsFine: false);

    public string ProjectNote
    {
        get
        {
            if (!AddProject)
            {
                return string.Empty;
            }

            return SetupValidator.CheckProject(BuildProject()).FirstOrDefault() switch
            {
                ProjectProblem.NameMissing => "Bitte einen Namen angeben.",
                ProjectProblem.RootMissing => "Der Projektordner existiert nicht.",
                ProjectProblem.RootInvalid => "Der Projektordner muss ein vollständiger Ordnerpfad sein.",
                ProjectProblem.SourceTextureDirectoryInvalid => "Quellbilder: ein relativer Pfad im Projektordner ist nötig.",
                ProjectProblem.AssetsDirectoryInvalid => "Asset-Ordner: ein relativer Pfad im Projektordner ist nötig.",
                ProjectProblem.TextureAssetDirectoryInvalid => "Texturen: ein relativer Pfad im Asset-Ordner ist nötig.",
                ProjectProblem.MaterialDirectoryInvalid => "Materialien: ein relativer Pfad im Asset-Ordner ist nötig.",
                _ => string.Empty,
            };
        }
    }

    private bool CanGoNext() => Step switch
    {
        0 => SetupValidator.CheckFolder(LibraryRoot) is FolderStatus.Exists or FolderStatus.Missing,
        1 => SetupValidator.CheckFolder(DownloadsDirectory) is FolderStatus.Exists,
        2 => !AddProject || SetupValidator.CheckProject(BuildProject()).Count == 0,
        _ => true,
    };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(Step))
        {
            OnPropertyChanged(nameof(IsLibraryStep));
            OnPropertyChanged(nameof(IsDownloadsStep));
            OnPropertyChanged(nameof(IsProjectStep));
            OnPropertyChanged(nameof(IsFlowStep));
            OnPropertyChanged(nameof(IsLastStep));
            OnPropertyChanged(nameof(IsNotFirstStep));
            OnPropertyChanged(nameof(StepTitle));
        }

        if (e.PropertyName is not (nameof(DialogResult) or nameof(LibraryNote) or nameof(DownloadsNote) or nameof(ProjectNote)))
        {
            OnPropertyChanged(nameof(LibraryNote));
            OnPropertyChanged(nameof(DownloadsNote));
            OnPropertyChanged(nameof(ProjectNote));
            NextCommand.NotifyCanExecuteChanged();
            FinishCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void BrowseLibrary() => LibraryRoot = dialogs.PickFolder(LibraryRoot) ?? LibraryRoot;

    [RelayCommand]
    private void BrowseDownloads() => DownloadsDirectory = dialogs.PickFolder(DownloadsDirectory) ?? DownloadsDirectory;

    [RelayCommand]
    private void BrowseProject()
    {
        if (dialogs.PickFolder(ProjectRootDirectory) is not { } folder)
        {
            return;
        }

        var suggestion = SetupSuggestions.ForProject(folder);
        ProjectRootDirectory = suggestion.RootDirectory;
        ProjectName = suggestion.Name;
        ProjectSourceTextureDirectory = suggestion.SourceTextureDirectory;
        ProjectAssetsDirectory = suggestion.AssetsDirectory;
        ProjectTextureAssetDirectory = suggestion.TextureAssetDirectory;
        ProjectMaterialDirectory = suggestion.MaterialDirectory;
    }

    [RelayCommand]
    private void Back() => Step = Math.Max(0, Step - 1);

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() => Step = Math.Min(lastStep, Step + 1);

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Finish()
    {
        try
        {
            var settings = guide.Apply(session.Settings, LibraryRoot, DownloadsDirectory, AddProject ? BuildProject() : null);
            session.SaveSettings(settings);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            dialogs.ShowError($"Die Einstellungen konnten nicht übernommen werden: {exception.Message}");
            return;
        }

        DialogResult = true;
    }

    [RelayCommand]
    private void Skip() => DialogResult = false;

    private StrideProject BuildProject() =>
        new(ProjectName, ProjectRootDirectory, ProjectSourceTextureDirectory, ProjectAssetsDirectory, ProjectTextureAssetDirectory, ProjectMaterialDirectory);

    private static string FolderNote(FolderStatus status, bool missingIsFine) => status switch
    {
        FolderStatus.Exists => "Ordner gefunden.",
        FolderStatus.Missing when missingIsFine => "Der Ordner gibt es noch nicht, er wird angelegt.",
        FolderStatus.Missing => "Diesen Ordner gibt es nicht.",
        FolderStatus.Empty => "Bitte einen Ordner angeben.",
        FolderStatus.IsFile => "Das ist eine Datei, kein Ordner.",
        _ => "Das ist kein vollständiger Ordnerpfad.",
    };
}
