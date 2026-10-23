using MaterialDepot.Core.Textures;

namespace MaterialDepot.Core.Downloads;

/// <summary>
/// One map file found among the downloads, either loose or inside a zip.
/// </summary>
/// <param name="FileName">The file's name, which it keeps in the library.</param>
/// <param name="Map">The map it holds.</param>
/// <param name="Location">The loose file, or the zip it is in.</param>
/// <param name="ZipEntry">The entry inside the zip; <see langword="null"/> for a loose file.</param>
/// <param name="Length">Size in bytes.</param>
public sealed record DownloadedMap(string FileName, PbrMap Map, string Location, string? ZipEntry, long Length)
{
    public bool IsInZip => ZipEntry is not null;
}
