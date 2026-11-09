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
    private readonly IProjectWorktreeFinder worktreeFinder;
    private readonly IDialogService dialogs;
    private readonly ObservableCollection<TextureSetCardViewModel> sets = [];
    private IReadOnlySet<string> projectStems = new HashSet<string>();

    public LibraryViewModel(
        DepotSession session,
        ThumbnailLoader thumbnails,
        IStrideProjectInventory inventory,
        IStrideMaterialImporter importer,
        IProjectWorktreeFinder worktreeFinder,
        IDialogService dialogs)
    {
        this.session = session;
        this.thumbnails = thumbnails;
        this.inventory = inventory;
        this.importer = importer;
        this.worktreeFinder = worktreeFinder;
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
    [NotifyPropertyChangedFor(nameof(TargetProject))]
    public partial StrideProject? SelectedProject { get; set; }

    /// <summary>
    /// The worktrees of the selected project's repository; empty when the project is not in a Git repository.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWorktrees))]
    public partial IReadOnlyList<WorktreeOption> Worktrees { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetProject))]
    public partial WorktreeOption? SelectedWorktree { get; set; }

    /// <summary>
    /// Whether there is a worktree to choose besides the configured one.
    /// </summary>
    public bool HasWorktrees => Worktrees.Count > 1;

    /// <summary>
    /// The project in the chosen worktree: what the import writes to and the "in project" marks are read from.
    /// </summary>
    public StrideProject? TargetProject => SelectedWorktree?.Target.Project ?? SelectedProject;

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
            ShowWorktrees();
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
        if (SelectedSet is not { } card || TargetProject is not { } project)
        {
            return;
        }

        var import = new ImportViewModel(card.Set, project, importer, dialogs) { Thumbnail = card.Thumbnail, TargetName = TargetName(project) };
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

    partial void OnSelectedProjectChanged(StrideProject? value) => ShowWorktrees();

    partial void OnSelectedWorktreeChanged(WorktreeOption? value) => _ = RefreshProjectStemsAsync();

    /// <summary>
    /// Reads the worktrees of the selected project's repository anew; keeps the chosen one if it is still there, else
    /// chooses the one the configured project root lies in.
    /// </summary>
    private void ShowWorktrees()
    {
        var chosen = SelectedWorktree?.Directory;
        List<WorktreeOption> options = SelectedProject is { } project
            ? [.. worktreeFinder.Find(project).Select(target => new WorktreeOption(target))]
            : [];
        Worktrees = options;
        var selected = options.FirstOrDefault(option => string.Equals(option.Directory, chosen, StringComparison.OrdinalIgnoreCase))
            ?? options.FirstOrDefault(option => option.Target.IsConfigured);
        if (Equals(selected, SelectedWorktree))
        {
            _ = RefreshProjectStemsAsync();
        }
        else
        {
            SelectedWorktree = selected;
        }
    }

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
        var project = TargetProject;
        var stems = project is null ? new HashSet<string>() : await Task.Run(() => inventory.ImportedStems(project));
        if (!Equals(project, TargetProject))
        {
            return;
        }

        projectStems = stems;
        foreach (var card in sets)
        {
            card.IsInProject = IsInProject(card.Set.Albedo?.Stem);
        }

        UpdateSummary();
    }

    /// <summary>
    /// The project's name, with the branch when there are worktrees to choose from.
    /// </summary>
    private string TargetName(StrideProject project) =>
        HasWorktrees && SelectedWorktree is { } worktree
            ? string.Format(Strings.Library_ProjectInWorktree, project.Name, worktree.Branch)
            : project.Name;

    private bool IsInProject(string? albedoStem) => albedoStem is not null && projectStems.Contains(albedoStem);

    private void UpdateSummary()
    {
        var shown = sets.Count(card => card.Matches(SearchText));
        var inProject = sets.Count(card => card.IsInProject);
        var projectPart = TargetProject is { } project ? string.Format(Strings.Library_SummaryInProject, inProject, TargetName(project)) : string.Empty;
        Summary = shown == sets.Count
            ? string.Format(Strings.Library_Summary, sets.Count, projectPart)
            : string.Format(Strings.Library_SummaryFiltered, shown, sets.Count, projectPart);
    }
}
