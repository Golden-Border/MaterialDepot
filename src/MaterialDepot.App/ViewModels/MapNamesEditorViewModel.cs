using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// The names one map goes by in file names, as a comma separated line in the settings.
/// </summary>
public sealed partial class MapNamesEditorViewModel(PbrMap map, IEnumerable<string> names) : ObservableObject
{
    public PbrMap Map { get; } = map;

    public string Label => MapLabels.Of(Map);

    [ObservableProperty]
    public partial string Text { get; set; } = string.Join(", ", names);

    public IReadOnlyList<string> Names => [.. Text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];
}
