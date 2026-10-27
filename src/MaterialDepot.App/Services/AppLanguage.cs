using System.Globalization;
using MaterialDepot.Core.Settings;

namespace MaterialDepot.App.Services;

/// <summary>
/// The UI language: the one from the settings, else the one of Windows, else English.
/// </summary>
/// <param name="system">The display language of Windows, taken before <see cref="Apply"/> changes it.</param>
public sealed class AppLanguage(CultureInfo system)
{
    public CultureInfo System { get; } = system;

    /// <summary>
    /// The language name the UI shows for the <paramref name="chosen"/> setting.
    /// </summary>
    public string Resolve(string? chosen) => UiLanguages.Resolve(chosen, System);

    /// <summary>
    /// Switches the UI culture. Only texts made afterwards are in the new language, so it is applied before the first window.
    /// </summary>
    public void Apply(string? chosen)
    {
        var culture = CultureInfo.GetCultureInfo(Resolve(chosen));
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
