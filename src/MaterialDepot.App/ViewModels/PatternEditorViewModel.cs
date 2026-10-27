using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDepot.App.Resources;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// One file name pattern in the settings, editable, with the reason it cannot be used.
/// </summary>
public sealed partial class PatternEditorViewModel(FilePattern pattern) : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = pattern.Name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Error), nameof(HasError))]
    public partial string Text { get; set; } = pattern.Regex;

    public string? Error => MapFileParser.Validate(Text) switch
    {
        null => null,
        PatternProblem.Empty => Strings.Pattern_Empty,
        PatternProblem.NotARegex => Strings.Pattern_NotARegex,
        PatternProblem.NameGroupMissing => string.Format(Strings.Pattern_GroupMissing, "name"),
        PatternProblem.MapGroupMissing => string.Format(Strings.Pattern_GroupMissing, "map"),
        _ => Strings.Pattern_NameAndMapGroupsMissing,
    };

    public bool HasError => Error is not null;

    public FilePattern ToPattern() => new(Name.Trim(), Text.Trim());
}
