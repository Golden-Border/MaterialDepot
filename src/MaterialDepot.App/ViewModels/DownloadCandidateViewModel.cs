using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Downloads;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// One downloaded set in the inbox.
/// </summary>
public sealed partial class DownloadCandidateViewModel(DownloadCandidate candidate) : ObservableObject
{
    public DownloadCandidate Candidate { get; } = candidate;

    public string BaseName => Candidate.BaseName;

    public IReadOnlyList<string> Maps { get; } = MapLabels.Of(candidate.Maps);

    public string Resolution { get; } = MapLabels.ResolutionOf(candidate.BaseName);

    public string Origin { get; } = string.Join(", ", candidate.SourceFiles.Select(Path.GetFileName).Distinct().Take(3))
        + (candidate.SourceFiles.Count() > 3 ? ", …" : string.Empty);

    public string Size { get; } = FormatSize(candidate.Files.Sum(file => file.Length));

    public string Status => Candidate.LibrarySetName is { } set ? $"Schon in der Bibliothek: {set}" : "Neu";

    /// <summary>
    /// The library set the files are already in, or else the suggested name.
    /// </summary>
    public string DefaultSetName { get; } = candidate.LibrarySetName ?? candidate.SuggestedName;

    [ObservableProperty]
    public partial string SetName { get; set; } = candidate.LibrarySetName ?? candidate.SuggestedName;

    [ObservableProperty]
    public partial ImageSource? Thumbnail { get; set; }

    public string PlaceholderText => Candidate.Files.FirstOrDefault(file => file.Map == PbrMap.Albedo) switch
    {
        null => "kein Albedo",
        { IsInZip: true } => "im Zip",
        _ => "Vorschau wird erstellt …",
    };

    public async Task LoadThumbnailAsync(ThumbnailLoader loader)
    {
        // Only a loose albedo gets a preview; one inside a zip would have to be unpacked first.
        var albedo = Candidate.Files.FirstOrDefault(file => file.Map == PbrMap.Albedo && !file.IsInZip);
        if (albedo is not null)
        {
            Thumbnail = await loader.LoadAsync(albedo.Location, urgent: true);
        }
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):0.0} MB",
        >= 1024 => $"{bytes / 1024.0:0} KB",
        _ => $"{bytes} B",
    };
}
