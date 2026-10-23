namespace MaterialDepot.Core.Stride;

/// <summary>
/// Where a Stride project keeps its textures and materials.
/// </summary>
/// <param name="Name">Display name.</param>
/// <param name="RootDirectory">The repository root.</param>
/// <param name="SourceTextureDirectory">Folder of the source images, relative to the root; one subfolder per texture set.</param>
/// <param name="AssetsDirectory">The Stride package's asset folder, relative to the root.</param>
/// <param name="TextureAssetDirectory">Folder of the <c>.sdtex</c> files, relative to the asset folder; one subfolder per texture set.</param>
/// <param name="MaterialDirectory">Default folder of the <c>.sdmat</c> files, relative to the asset folder.</param>
public sealed record StrideProject(
    string Name,
    string RootDirectory,
    string SourceTextureDirectory,
    string AssetsDirectory,
    string TextureAssetDirectory,
    string MaterialDirectory)
{
    public string AssetsPath => Path.GetFullPath(Path.Combine(RootDirectory, AssetsDirectory));

    public string SourceTexturePath(string textureSet) => Path.GetFullPath(Path.Combine(RootDirectory, SourceTextureDirectory, textureSet));

    public string TextureAssetPath(string textureSet) => Path.GetFullPath(Path.Combine(AssetsPath, TextureAssetDirectory, textureSet));

    public string MaterialPath(string materialDirectory, string materialName) => Path.GetFullPath(Path.Combine(AssetsPath, materialDirectory, materialName + ".sdmat"));
}
