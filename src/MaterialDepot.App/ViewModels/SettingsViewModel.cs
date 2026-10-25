using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Settings;
using MaterialDepot.Core.Setup;
using MaterialDepot.Core.Stride;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The settings tab: library and download folder, and the Stride projects to import into.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly DepotSession session;
    private readonly IDialogService dialogs;
    private readonly SetupGuide setupGuide;

    public SettingsViewModel(DepotSession session, SetupGuide setupGuide, IDialogService dialogs)
    {
        this.session = session;
        this.setupGuide = setupGuide;
        this.dialogs = dialogs;
        Reset();
    }

    public ObservableCollection<ProjectEditorViewModel> Projects { get; } = [];

    [ObservableProperty]
    public partial string LibraryRoot { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DownloadsDirectory { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveProjectCommand))]
    public partial ProjectEditorViewModel? SelectedProject { get; set; }

    [ObservableProperty]
    public partial string SavedNote { get; set; } = string.Empty;

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
    private async Task SaveAsync()
    {
        var libraryChanged = session.Settings.LibraryRoot != LibraryRoot.Trim();
        session.SaveSettings(new DepotSettings(LibraryRoot.Trim(), DownloadsDirectory.Trim(), [.. Projects.Select(project => project.ToProject())]));
        SavedNote = "Gespeichert.";
        if (libraryChanged)
        {
            await session.RescanLibraryAsync();
        }
    }

    [RelayCommand]
    private async Task RunSetupAsync()
    {
        if (dialogs.ShowSetup(new SetupViewModel(session, setupGuide, dialogs)))
        {
            Reset();
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
        SavedNote = string.Empty;
    }

    private bool HasSelectedProject() => SelectedProject is not null;
}
