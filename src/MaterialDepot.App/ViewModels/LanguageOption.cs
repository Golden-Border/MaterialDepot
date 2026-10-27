namespace MaterialDepot.App.ViewModels;

/// <summary>
/// One entry of the language list in the settings.
/// </summary>
/// <param name="Language">The language name to save, <see langword="null"/> to follow Windows.</param>
/// <param name="Label">What the list shows; a language by its own name, so it can be found whatever the UI shows.</param>
public sealed record LanguageOption(string? Language, string Label)
{
    /// <summary>
    /// The label, which is also what UI Automation reads out for the list entry.
    /// </summary>
    public override string ToString() => Label;
}
