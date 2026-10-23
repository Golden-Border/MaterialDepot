using ImageMagick;

namespace MaterialDepot.Core.Tests;

/// <summary>
/// Synthetic texture images. Store-bought textures never go into this repo.
/// </summary>
internal static class TestImages
{
    public static string WriteTiff(string path, uint size = 8, uint depth = 8, string color = "#806040")
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        using var image = new MagickImage(new MagickColor(color), size, size);
        image.Depth = depth;
        image.Write(path, MagickFormat.Tiff);
        return path;
    }
}
