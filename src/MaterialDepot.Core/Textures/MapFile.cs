namespace MaterialDepot.Core.Textures;

/// <summary>
/// An image file that holds one map of a texture set, recognised by its name (see <see cref="MapFileParser"/>).
/// </summary>
/// <param name="Path">Full path of the file.</param>
/// <param name="BaseName">What the maps of one set share: the file name without the map, the resolution after it and the extension.</param>
/// <param name="Map">The map the file holds.</param>
public sealed record MapFile(string Path, string BaseName, PbrMap Map)
{
    /// <summary>
    /// The file name without its extension, which an imported copy keeps.
    /// </summary>
    public string Stem => System.IO.Path.GetFileNameWithoutExtension(Path);
}
