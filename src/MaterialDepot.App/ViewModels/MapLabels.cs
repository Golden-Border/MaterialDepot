using System.Text.RegularExpressions;
using MaterialDepot.Core.Textures;

namespace MaterialDepot.App.ViewModels;

/// <summary>
/// Short labels for what a set brings.
/// </summary>
public static partial class MapLabels
{
    public static string Of(PbrMap map) => map switch
    {
        PbrMap.Albedo => "Albedo",
        PbrMap.Normal => "Normal",
        PbrMap.Roughness => "Rough",
        PbrMap.Metallic => "Metal",
        PbrMap.AmbientOcclusion => "AO",
        PbrMap.Height => "Height",
        _ => map.ToString(),
    };

    public static IReadOnlyList<string> Of(IEnumerable<PbrMap> maps) => [.. maps.Order().Select(Of)];

    /// <summary>
    /// The resolution token of a file name, such as <c>1K</c>, or an empty string.
    /// </summary>
    public static string ResolutionOf(string fileName)
    {
        var match = Resolution().Match(fileName);
        return match.Success ? match.Groups["resolution"].Value.ToUpperInvariant() : string.Empty;
    }

    [GeneratedRegex(@"[_-](?<resolution>\d+k)(?:[_-]|$)", RegexOptions.IgnoreCase)]
    private static partial Regex Resolution();
}
