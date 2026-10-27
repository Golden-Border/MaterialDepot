using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDepot.App.Resources;
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
        0 => Strings.Setup_StepLibrary,
        1 => Strings.Setup_StepDownloads,
        2 => Strings.Setup_StepProject,
        _ => Strings.Setup_StepFlow,
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
                ProjectProblem.NameMissing => Strings.Setup_NameMissing,
                ProjectProblem.RootMissing => Strings.Setup_RootMissing,
                ProjectProblem.RootInvalid => Strings.Setup_RootInvalid,
                ProjectProblem.SourceTextureDirectoryInvalid => Strings.Setup_SourceTextureDirectoryInvalid,
                ProjectProblem.AssetsDirectoryInvalid => Strings.Setup_AssetsDirectoryInvalid,
                ProjectProblem.TextureAssetDirectoryInvalid => Strings.Setup_TextureAssetDirectoryInvalid,
                ProjectProblem.MaterialDirectoryInvalid => Strings.Setup_MaterialDirectoryInvalid,
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
            dialogs.ShowError(string.Format(Strings.Setup_NotApplied, exception.Message));
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
        FolderStatus.Exists => Strings.Folder_Found,
        FolderStatus.Missing when missingIsFine => Strings.Folder_WillBeCreated,
        FolderStatus.Missing => Strings.Folder_Missing,
        FolderStatus.Empty => Strings.Folder_Empty,
        FolderStatus.IsFile => Strings.Folder_IsFile,
        _ => Strings.Folder_Invalid,
    };
}
