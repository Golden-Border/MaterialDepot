namespace MaterialDepot.Core.Textures;

/// <summary>
/// The maps of one PBR material, as they lie in a folder.
/// </summary>
/// <param name="Name">The display name; in the library the folder name.</param>
/// <param name="Directory">The folder the files are in.</param>
/// <param name="Maps">One file per map.</param>
public sealed record TextureSet(string Name, string Directory, IReadOnlyDictionary<PbrMap, MapFile> Maps)
{
    public MapFile? Albedo => Maps.GetValueOrDefault(PbrMap.Albedo);
}
