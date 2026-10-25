using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDepot.Core.Imaging;

namespace MaterialDepot.App.Services;

/// <summary>
/// Previews as WPF images, made off the UI thread.
/// </summary>
public sealed class ThumbnailLoader(IThumbnailCache cache)
{
    // Decoding a 4K TIFF takes a moment and a lot of memory; a few at a time keep the machine usable.
    private readonly SemaphoreSlim throttle = new(Environment.ProcessorCount / 2 + 1);

    // The whole library queues up on the first start; a new download should not wait behind it.
    private readonly SemaphoreSlim urgentThrottle = new(2);

    /// <summary>
    /// The preview of <paramref name="imagePath"/>, or <see langword="null"/> when the image cannot be read.
    /// An <paramref name="urgent"/> one does not queue behind the others.
    /// </summary>
    public async Task<ImageSource?> LoadAsync(string imagePath, bool urgent = false)
    {
        var throttle = urgent ? urgentThrottle : this.throttle;
        await throttle.WaitAsync();
        try
        {
            return await Task.Run(() => Load(imagePath));
        }
        finally
        {
            throttle.Release();
        }
    }

    private ImageSource? Load(string imagePath)
    {
        try
        {
            var thumbnail = cache.GetOrCreate(imagePath);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(thumbnail);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ImageMagick.MagickException or NotSupportedException)
        {
            return null;
        }
    }
}
