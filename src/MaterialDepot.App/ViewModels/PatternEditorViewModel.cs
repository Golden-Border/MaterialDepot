using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// One file name pattern in the settings, editable, with the reason it cannot be used.
/// </summary>
public sealed partial class PatternEditorViewModel(string pattern) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Error), nameof(HasError))]
    public partial string Text { get; set; } = pattern;

    public string? Error => MapFileParser.Validate(Text);

    public bool HasError => Error is not null;
}
