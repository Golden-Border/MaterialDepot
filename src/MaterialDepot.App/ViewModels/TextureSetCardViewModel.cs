using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDepot.App.Resources;
using MaterialDepot.App.Services;
using MaterialDepot.Core.Library;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// One tile of the library grid.
/// </summary>
public sealed partial class TextureSetCardViewModel(TextureSet set, string libraryRoot) : ObservableObject
{
    public TextureSet Set { get; } = set;

    public string Name => Set.Name;

    /// <summary>
    /// Where the set lies inside the library, so two sets of the same name can be told apart.
    /// </summary>
    public string Location { get; } = Path.GetRelativePath(libraryRoot, set.Directory);

    public IReadOnlyList<string> Maps { get; } = MapLabels.Of(set.Maps.Keys);

    public string Resolution { get; } = set.Albedo is { } albedo ? MapLabels.ResolutionOf(albedo.Stem) : string.Empty;

    public bool HasAlbedo => Set.Albedo is not null;

    /// <summary>
    /// Whether the set has the maps a material needs.
    /// </summary>
    public bool CanImport => Set.Maps.ContainsKey(PbrMap.Albedo) && Set.Maps.ContainsKey(PbrMap.Normal) && Set.Maps.ContainsKey(PbrMap.Roughness);

    /// <summary>
    /// Whether one of the maps lies only in the sync client's cloud; an import would download it first.
    /// </summary>
    public bool HasCloudOnlyMaps { get; } = set.Maps.Values.Any(map => FileAvailability.IsCloudOnly(map.Path));

    [ObservableProperty]
    public partial string PlaceholderText { get; set; } = set.Albedo is null ? Strings.Preview_NoAlbedo : Strings.Preview_Loading;

    [ObservableProperty]
    public partial ImageSource? Thumbnail { get; set; }

    [ObservableProperty]
    public partial bool IsInProject { get; set; }

    public async Task LoadThumbnailAsync(ThumbnailLoader loader)
    {
        if (Set.Albedo is not { } albedo || Thumbnail is not null)
        {
            return;
        }

        // A cloud-only albedo is not read: browsing the library would otherwise download every set.
        if (FileAvailability.IsCloudOnly(albedo.Path))
        {
            PlaceholderText = Strings.Preview_CloudOnly;
            return;
        }

        Thumbnail = await loader.LoadAsync(albedo.Path);
        if (Thumbnail is null)
        {
            PlaceholderText = Strings.Preview_Failed;
        }
    }

    public bool Matches(string search) =>
        string.IsNullOrWhiteSpace(search)
        || Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)
        || Location.Contains(search, StringComparison.CurrentCultureIgnoreCase)
        || Set.Maps.Values.Any(map => map.Stem.Contains(search, StringComparison.CurrentCultureIgnoreCase));
}
