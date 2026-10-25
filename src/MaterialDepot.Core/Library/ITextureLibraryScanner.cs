using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Library;

/// <summary>
/// Finds the texture sets in the library folder.
/// </summary>
public interface ITextureLibraryScanner
{
    /// <summary>
    /// Every set under <paramref name="root"/>, sorted by name. A folder whose maps occur once each is one set named
    /// after the folder, even when its files have different base names; a folder with two files for the same map holds
    /// one set per base name. Files that <paramref name="parser"/> does not take for a map are ignored.
    /// </summary>
    IReadOnlyList<TextureSet> Scan(string root, MapFileParser parser);
}
