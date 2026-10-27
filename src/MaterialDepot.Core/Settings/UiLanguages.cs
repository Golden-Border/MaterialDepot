using System.Globalization;

namespace MaterialDepot.Core.Settings;

/// <summary>
/// The languages the UI is translated into, and which of them it shows.
/// </summary>
public static class UiLanguages
{
    /// <summary>
    /// The language for everything without a translation.
    /// </summary>
    public const string Fallback = "en";

    /// <summary>
    /// The language names (<c>en</c>, <c>de</c>) there are translations for, the fallback first.
    /// </summary>
    public static IReadOnlyList<string> Supported { get; } = [Fallback, "de"];

    /// <summary>
    /// The language the UI shows: the <paramref name="chosen"/> one when there is a translation for it, otherwise the
    /// <paramref name="system"/> language or its neutral parent (<c>de-AT</c> gives <c>de</c>), otherwise English.
    /// </summary>
    /// <param name="chosen">The language from the settings; <see langword="null"/> follows the system.</param>
    /// <param name="system">The display language of Windows.</param>
    public static string Resolve(string? chosen, CultureInfo system)
    {
        if (Find(chosen) is { } language)
        {
            return language;
        }

        for (var culture = system; !string.IsNullOrEmpty(culture.Name); culture = culture.Parent)
        {
            if (Find(culture.Name) is { } systemLanguage)
            {
                return systemLanguage;
            }
        }

        return Fallback;
    }

    private static string? Find(string? name) =>
        Supported.FirstOrDefault(language => string.Equals(language, name, StringComparison.OrdinalIgnoreCase));
}
