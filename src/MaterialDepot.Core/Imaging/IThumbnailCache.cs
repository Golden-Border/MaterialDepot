namespace MaterialDepot.Core.Imaging;

/// <summary>
/// Small PNG previews of texture images, made once and kept on disk.
/// </summary>
public interface IThumbnailCache
{
    /// <summary>
    /// The path of a preview of <paramref name="imagePath"/>. A preview is made again when the image changes.
    /// </summary>
    string GetOrCreate(string imagePath);
}
