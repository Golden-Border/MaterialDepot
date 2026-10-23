using System.Security.Cryptography;
using System.Text;

namespace MaterialDepot.Core.Imaging;

/// <summary>
/// Keeps previews in <paramref name="cacheDirectory"/>, named after the image's path, size and change time.
/// </summary>
public sealed class ThumbnailCache(IImageConverter imageConverter, string cacheDirectory, uint size = 256) : IThumbnailCache
{
    public string GetOrCreate(string imagePath)
    {
        var image = new FileInfo(imagePath);
        var key = $"{image.FullName.ToUpperInvariant()}|{image.Length}|{image.LastWriteTimeUtc.Ticks}|{size}";
        var name = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..32] + ".png";
        var thumbnail = Path.Combine(cacheDirectory, name);
        if (File.Exists(thumbnail))
        {
            return thumbnail;
        }

        Directory.CreateDirectory(cacheDirectory);
        // Written beside and renamed, so a preview being made by another thread is never read half-written.
        var partial = thumbnail + "." + Guid.NewGuid().ToString("N") + ".tmp";
        imageConverter.WriteThumbnail(imagePath, partial, size);
        File.Move(partial, thumbnail, overwrite: true);
        return thumbnail;
    }
}
