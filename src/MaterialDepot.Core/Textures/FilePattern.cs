namespace MaterialDepot.Core.Textures;

/// <summary>
/// A regular expression that recognises map files, with a readable name for the settings.
/// </summary>
/// <param name="Name">What the pattern is for, such as the store whose files it matches.</param>
/// <param name="Regex">The expression, see <see cref="MapFileParser"/>.</param>
public sealed record FilePattern(string Name, string Regex);
