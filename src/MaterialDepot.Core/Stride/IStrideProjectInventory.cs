namespace MaterialDepot.Core.Stride;

/// <summary>
/// Which source images a Stride project already uses.
/// </summary>
public interface IStrideProjectInventory
{
    /// <summary>
    /// The file names, without extension, of every image a texture asset of <paramref name="project"/> points at.
    /// A library set is in the project when the stem of its albedo is among them.
    /// </summary>
    IReadOnlySet<string> ImportedStems(StrideProject project);
}
