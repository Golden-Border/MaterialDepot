using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Stride;

/// <summary>
/// What to import into which project, and under which names.
/// </summary>
/// <param name="Set">The texture set from the library.</param>
/// <param name="Project">The target project.</param>
/// <param name="TextureFolderName">Subfolder for the source images and the <c>.sdtex</c> files; by convention the product name, such as <c>Heavy Rust 3</c>.</param>
/// <param name="MaterialName">File name of the <c>.sdmat</c>, without extension.</param>
/// <param name="MaterialDirectory">Folder of the <c>.sdmat</c>, relative to the project's asset folder.</param>
/// <param name="UvScale">Tiling of the material.</param>
/// <param name="MaterialId">Id for the material when models already reference one that is not on disk yet; otherwise an existing material keeps its own and a new one gets a fresh one.</param>
public sealed record StrideImportRequest(
    TextureSet Set,
    StrideProject Project,
    string TextureFolderName,
    string MaterialName,
    string MaterialDirectory,
    double UvScale,
    Guid? MaterialId = null)
{
    /// <summary>
    /// The UV scale for a texture that covers <paramref name="tileSizeMeters"/> on a mesh whose UVs give
    /// <paramref name="metersPerUvUnit"/> per unit: a 20 cm tile on models with 0.5 m per UV unit needs 2.5.
    /// </summary>
    public static double UvScaleFor(double tileSizeMeters, double metersPerUvUnit) => metersPerUvUnit / tileSizeMeters;
}
