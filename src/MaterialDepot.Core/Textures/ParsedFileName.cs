namespace MaterialDepot.Core.Textures;

/// <summary>
/// What a file name pattern extracted from a file name.
/// </summary>
/// <param name="Name">The <c>name</c> group, shared by the maps of one set.</param>
/// <param name="MapToken">The <c>map</c> group as written in the file name.</param>
/// <param name="Map">The map the token stands for, or <see langword="null"/> when no map name matches it.</param>
/// <param name="Resolution">The optional <c>res</c> group.</param>
public sealed record ParsedFileName(string Name, string MapToken, PbrMap? Map, string? Resolution);
