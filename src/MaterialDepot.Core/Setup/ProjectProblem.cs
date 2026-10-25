namespace MaterialDepot.Core.Setup;

/// <summary>
/// Something that keeps a Stride project entry from working.
/// </summary>
public enum ProjectProblem
{
    NameMissing,
    RootMissing,
    RootInvalid,
    SourceTextureDirectoryInvalid,
    AssetsDirectoryInvalid,
    TextureAssetDirectoryInvalid,
    MaterialDirectoryInvalid,
}
