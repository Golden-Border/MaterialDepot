using ImageMagick;

namespace MaterialDepot.Core.Imaging;

public sealed class MagickImageConverter : IImageConverter
{
    public void ConvertToPng(string sourcePath, string targetPath)
    {
        using var image = new MagickImage(sourcePath);
        // Stride has no sRGB variant for 16-bit formats, so a 16-bit albedo renders too bright.
        image.Depth = 8;
        WritePng(image, targetPath);
    }

    public void WriteThumbnail(string sourcePath, string targetPath, uint size)
    {
        using var image = new MagickImage(sourcePath);
        image.Thumbnail(size, size);
        image.Depth = 8;
        WritePng(image, targetPath);
    }

    private static void WritePng(MagickImage image, string targetPath)
    {
        image.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", "date,time");
        image.Write(targetPath, MagickFormat.Png);
    }
}
