using System.Text.RegularExpressions;
using System.Xml.Linq;
using MaterialDepot.Core.Settings;

namespace MaterialDepot.Core.Tests.Localization;

/// <summary>
/// The UI texts of the app: every translation has the same keys and placeholders as the English Strings.resx.
/// </summary>
public sealed partial class StringsResourceTests
{
    private static readonly string resourceDirectory = Path.Combine(FindRepositoryRoot(), "src", "MaterialDepot.App", "Resources");

    public static TheoryData<string> Translations => [.. UiLanguages.Supported.Where(language => language != UiLanguages.Fallback)];

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_HasTheSameKeysAsEnglish(string language)
    {
        // Arrange
        var english = Read("Strings.resx");

        // Act
        var translated = Read($"Strings.{language}.resx");

        // Assert
        translated.Keys.Except(english.Keys).ShouldBeEmpty("keys that English does not have");
        english.Keys.Except(translated.Keys).ShouldBeEmpty("keys without a translation");
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_KeepsThePlaceholders(string language)
    {
        // Arrange
        var english = Read("Strings.resx");

        // Act
        var translated = Read($"Strings.{language}.resx");

        // Assert
        var mismatches = english.Keys.Intersect(translated.Keys)
            .Where(key => Placeholders(english[key]) != Placeholders(translated[key]))
            .ToList();
        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void English_HasNoEmptyTexts()
    {
        // Act & Assert
        Read("Strings.resx").Where(pair => string.IsNullOrWhiteSpace(pair.Value)).Select(pair => pair.Key).ShouldBeEmpty();
    }

    private static Dictionary<string, string> Read(string fileName) =>
        XDocument.Load(Path.Combine(resourceDirectory, fileName))
            .Root!
            .Elements("data")
            .ToDictionary(data => (string)data.Attribute("name")!, data => (string?)data.Element("value") ?? string.Empty);

    private static string Placeholders(string text) =>
        string.Join(",", Placeholder().Matches(text).Select(match => match.Value).Distinct().Order(StringComparer.Ordinal));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MaterialDepot.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("MaterialDepot.slnx not found above the test output.");
    }

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex Placeholder();
}
