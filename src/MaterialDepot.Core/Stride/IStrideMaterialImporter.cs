namespace MaterialDepot.Core.Stride;

/// <summary>
/// Makes a texture set a ready material of a Stride project.
/// </summary>
public interface IStrideMaterialImporter
{
    /// <summary>
    /// Writes the source PNGs, one <c>.sdtex</c> per map and the <c>.sdmat</c>. Running it again on the same set reproduces the same files.
    /// </summary>
    StrideImportResult Import(StrideImportRequest request);
}
