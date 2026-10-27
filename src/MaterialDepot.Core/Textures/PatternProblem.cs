namespace MaterialDepot.Core.Textures;

/// <summary>
/// Why a file name pattern cannot be used, see <see cref="MapFileParser.Validate"/>.
/// </summary>
public enum PatternProblem
{
    Empty,
    NotARegex,
    NameGroupMissing,
    MapGroupMissing,
    NameAndMapGroupsMissing,
}
