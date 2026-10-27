using System.Globalization;
using MaterialDepot.Core.Settings;

namespace MaterialDepot.Core.Tests.Settings;

public sealed class UiLanguagesTests
{
    [Theory]
    [InlineData("de-DE", "de")]
    [InlineData("de-AT", "de")]
    [InlineData("de", "de")]
    [InlineData("en-US", "en")]
    [InlineData("en-GB", "en")]
    public void Resolve_NothingChosen_FollowsTheSystem(string system, string expected)
    {
        // Act & Assert
        UiLanguages.Resolve(null, CultureInfo.GetCultureInfo(system)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("ja-JP")]
    [InlineData("")]
    public void Resolve_SystemWithoutTranslation_FallsBackToEnglish(string system)
    {
        // Act & Assert
        UiLanguages.Resolve(null, CultureInfo.GetCultureInfo(system)).ShouldBe("en");
    }

    [Theory]
    [InlineData("en", "de-DE", "en")]
    [InlineData("de", "en-US", "de")]
    [InlineData("DE", "fr-FR", "de")]
    public void Resolve_ChosenLanguage_WinsOverTheSystem(string chosen, string system, string expected)
    {
        // Act & Assert
        UiLanguages.Resolve(chosen, CultureInfo.GetCultureInfo(system)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("")]
    public void Resolve_ChosenLanguageWithoutTranslation_FollowsTheSystem(string chosen)
    {
        // Act & Assert
        UiLanguages.Resolve(chosen, CultureInfo.GetCultureInfo("de-DE")).ShouldBe("de");
    }

    [Fact]
    public void Supported_StartsWithTheFallback()
    {
        // Act & Assert
        UiLanguages.Supported[0].ShouldBe(UiLanguages.Fallback);
        UiLanguages.Fallback.ShouldBe("en");
    }
}
