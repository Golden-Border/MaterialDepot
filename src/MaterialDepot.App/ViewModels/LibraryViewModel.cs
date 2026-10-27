using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDepot.App.Resources;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Stride;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The library tab: every set with its albedo, a search, and the import into the chosen project.
/// </summary>
public sealed partial class LibraryViewModel : ObservableObject
{
    private readonly DepotSession session;
    private readonly ThumbnailLoader thumbnails;
    private readonly IStrideProjectInventory inventory;
    private readonly IStrideMaterialImporter importer;
    private readonly IDialogService dialogs;
    private readonly ObservableCollection<TextureSetCardViewModel> sets = [];
    private IReadOnlySet<string> projectStems = new HashSet<string>();

    public LibraryViewModel(
        DepotSession session,
        ThumbnailLoader thumbnails,
        IStrideProjectInventory inventory,
        IStrideMaterialImporter importer,
        IDialogService dialogs)
    {
        this.session = session;
        this.thumbnails = thumbnails;
        this.inventory = inventory;
        this.importer = importer;
        this.dialogs = dialogs;
        Sets = CollectionViewSource.GetDefaultView(sets);
        Sets.Filter = item => item is TextureSetCardViewModel card && card.Matches(SearchText);
        Projects = new ObservableCollection<StrideProject>(session.Settings.Projects);
        SelectedProject = Projects.FirstOrDefault();
        session.LibraryChanged += (_, _) => ShowLibrary();
        session.SettingsChanged += (_, _) => ShowProjects();
    }

    public ICollectionView Sets { get; }

    public ObservableCollection<StrideProject> Projects { get; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenFolderCommand))]
    public partial TextureSetCardViewModel? SelectedSet { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    public partial StrideProject? SelectedProject { get; set; }

    [ObservableProperty]
    public partial bool IsScanning { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [RelayCommand]
    private async Task RescanAsync()
    {
        IsScanning = true;
        try
        {
            await session.RescanLibraryAsync();
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanImport))]
    private void Import()
    {
        if (SelectedSet is not { } card || SelectedProject is not { } project)
        {
            return;
        }

        var import = new ImportViewModel(card.Set, project, importer, dialogs) { Thumbnail = card.Thumbnail };
        if (dialogs.ShowImport(import))
        {
            _ = RefreshProjectStemsAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void OpenFolder()
    {
        if (SelectedSet is { } card)
        {
            dialogs.OpenFolder(card.Set.Directory);
        }
    }

    private bool CanImport() => SelectedSet is { CanImport: true } && SelectedProject is not null;

    private bool HasSelection() => SelectedSet is not null;

    partial void OnSearchTextChanged(string value)
    {
        Sets.Refresh();
        UpdateSummary();
    }

    partial void OnSelectedProjectChanged(StrideProject? value) => _ = RefreshProjectStemsAsync();

    private void ShowLibrary()
    {
        var root = session.Settings.LibraryRoot;
        sets.Clear();
        foreach (var set in session.Library)
        {
            var card = new TextureSetCardViewModel(set, root) { IsInProject = IsInProject(set.Albedo?.Stem) };
            sets.Add(card);
            _ = card.LoadThumbnailAsync(thumbnails);
        }

        UpdateSummary();
    }

    private void ShowProjects()
    {
        var selected = SelectedProject?.Name;
        Projects.Clear();
        foreach (var project in session.Settings.Projects)
        {
            Projects.Add(project);
        }

        SelectedProject = Projects.FirstOrDefault(project => project.Name == selected) ?? Projects.FirstOrDefault();
    }

    private async Task RefreshProjectStemsAsync()
    {
        var project = SelectedProject;
        projectStems = project is null ? new HashSet<string>() : await Task.Run(() => inventory.ImportedStems(project));
        foreach (var card in sets)
        {
            card.IsInProject = IsInProject(card.Set.Albedo?.Stem);
        }

        UpdateSummary();
    }

    private bool IsInProject(string? albedoStem) => albedoStem is not null && projectStems.Contains(albedoStem);

    private void UpdateSummary()
    {
        var shown = sets.Count(card => card.Matches(SearchText));
        var inProject = sets.Count(card => card.IsInProject);
        var projectPart = SelectedProject is { } project ? string.Format(Strings.Library_SummaryInProject, inProject, project.Name) : string.Empty;
        Summary = shown == sets.Count
            ? string.Format(Strings.Library_Summary, sets.Count, projectPart)
            : string.Format(Strings.Library_SummaryFiltered, shown, sets.Count, projectPart);
    }
}
