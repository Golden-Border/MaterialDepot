namespace MaterialDepot.Core.Imaging;

/// <summary>
/// Turns texture images into the files the depot writes.
/// </summary>
public interface IImageConverter
{
    /// <summary>
    /// Writes <paramref name="sourcePath"/> as an 8-bit PNG without timestamps, so the same source always gives the same bytes.
    /// </summary>
    void ConvertToPng(string sourcePath, string targetPath);

    /// <summary>
    /// Writes a PNG preview of <paramref name="sourcePath"/> that fits into <paramref name="size"/> × <paramref name="size"/> pixels.
    /// </summary>
    void WriteThumbnail(string sourcePath, string targetPath, uint size);
}
