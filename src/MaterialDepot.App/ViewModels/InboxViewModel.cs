using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Downloads;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The inbox tab: texture sets in the download folder, to copy or move into the library, or to throw away.
/// </summary>
public sealed partial class InboxViewModel : ObservableObject
{
    private readonly DepotSession session;
    private readonly IDownloadScanner scanner;
    private readonly IDownloadArchiver archiver;
    private readonly ThumbnailLoader thumbnails;
    private readonly IDialogService dialogs;
    private readonly IFileRemover fileRemover;
    private int latestScan;

    public InboxViewModel(
        DepotSession session,
        IDownloadScanner scanner,
        IDownloadArchiver archiver,
        IFileRemover fileRemover,
        ThumbnailLoader thumbnails,
        IDialogService dialogs)
    {
        this.session = session;
        this.scanner = scanner;
        this.archiver = archiver;
        this.fileRemover = fileRemover;
        this.thumbnails = thumbnails;
        this.dialogs = dialogs;
        session.LibraryChanged += (_, _) => _ = RescanAsync();
    }

    public ObservableCollection<DownloadCandidateViewModel> Candidates { get; } = [];

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    /// <summary>
    /// How many candidates are not in the library yet, for the tab header.
    /// </summary>
    [ObservableProperty]
    public partial int NewCount { get; set; }

    [ObservableProperty]
    public partial string LastAction { get; set; } = string.Empty;

    [RelayCommand]
    private async Task RescanAsync()
    {
        var scan = ++latestScan;
        var downloads = session.Settings.DownloadsDirectory;
        var library = session.Library;
        var found = await Task.Run(() => scanner.Scan(downloads, library));
        if (scan != latestScan)
        {
            // A newer scan started meanwhile, for instance after the library changed; its result counts.
            return;
        }

        // The window rescans whenever it is activated, so a name typed before switching to the browser must survive.
        var typedNames = Candidates.Where(item => item.SetName != item.DefaultSetName)
            .ToDictionary(item => item.BaseName, item => item.SetName, StringComparer.OrdinalIgnoreCase);
        Candidates.Clear();
        foreach (var candidate in found)
        {
            var item = new DownloadCandidateViewModel(candidate);
            if (typedNames.TryGetValue(candidate.BaseName, out var typed))
            {
                item.SetName = typed;
            }

            Candidates.Add(item);
            _ = item.LoadThumbnailAsync(thumbnails);
        }

        NewCount = found.Count(candidate => !candidate.IsInLibrary);
        Summary = found.Count == 0
            ? $"Keine Texturen in {downloads}"
            : $"{found.Count} Sets in {downloads}, {NewCount} davon neu";
    }

    [RelayCommand]
    private Task CopyAsync(DownloadCandidateViewModel item) => ArchiveAsync(item, move: false);

    [RelayCommand]
    private Task MoveAsync(DownloadCandidateViewModel item) => ArchiveAsync(item, move: true);

    [RelayCommand]
    private async Task DeleteAsync(DownloadCandidateViewModel item)
    {
        var sources = item.Candidate.SourceFiles.ToList();
        var zipNote = item.Candidate.Files.Any(file => file.IsInZip)
            ? "\n\nEin Zip landet als Ganzes im Papierkorb, auch was sonst noch darin ist."
            : string.Empty;
        if (!dialogs.Confirm(
            $"„{item.BaseName}“ aus den Downloads in den Papierkorb legen?\n\n"
            + string.Join("\n", sources.Select(Path.GetFileName))
            + zipNote))
        {
            return;
        }

        IsBusy = true;
        try
        {
            await Task.Run(() => sources.ForEach(fileRemover.Remove));
            LastAction = $"{sources.Count} Downloads von „{item.BaseName}“ in den Papierkorb.";
            await RescanAsync();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or OperationCanceledException)
        {
            dialogs.ShowError($"Das Löschen ist fehlgeschlagen:\n{exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ArchiveAsync(DownloadCandidateViewModel item, bool move)
    {
        var setName = item.SetName.Trim();
        var libraryRoot = session.Settings.LibraryRoot;
        if (setName.Length == 0 || setName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            dialogs.ShowError($"„{item.SetName}“ taugt nicht als Ordnername.");
            return;
        }

        IsBusy = true;
        try
        {
            var result = await Task.Run(() => archiver.Archive(item.Candidate, setName, libraryRoot, move));
            LastAction = Describe(result, setName);
            if (result.Conflicts.Count > 0)
            {
                dialogs.ShowError(
                    "Diese Dateien liegen schon mit anderem Inhalt in der Bibliothek und wurden nicht überschrieben:\n"
                    + string.Join("\n", result.Conflicts.Select(Path.GetFileName))
                    + (move ? "\n\nDie Downloads bleiben deshalb liegen." : string.Empty));
            }

            await session.RescanLibraryAsync();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            dialogs.ShowError($"Das Einsortieren ist fehlgeschlagen:\n{exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string Describe(ArchiveResult result, string setName)
    {
        var parts = new List<string> { $"{result.Copied.Count} Dateien nach „{setName}“ kopiert" };
        if (result.AlreadyThere.Count > 0)
        {
            parts.Add($"{result.AlreadyThere.Count} waren schon da");
        }

        if (result.Removed.Count > 0)
        {
            parts.Add($"{result.Removed.Count} Downloads in den Papierkorb");
        }

        return string.Join(", ", parts) + ".";
    }
}
