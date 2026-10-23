using ImageMagick;
using MaterialDepot.Core.Imaging;
using Moq;

namespace MaterialDepot.Core.Tests.Imaging;

public sealed class ThumbnailCacheTests : IDisposable
{
    private readonly TempDirectory temp = new();
    private readonly Mock<IImageConverter> converter = new();
    private readonly ThumbnailCache sut;

    public ThumbnailCacheTests()
    {
        converter.Setup(c => c.WriteThumbnail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<uint>()))
            .Callback<string, string, uint>((_, target, _) => File.WriteAllBytes(target, [1]));
        sut = new ThumbnailCache(converter.Object, temp.Combine("cache"), 128);
    }

    public void Dispose() => temp.Dispose();

    [Fact]
    public void GetOrCreate_SecondCall_ReusesThePreview()
    {
        // Arrange
        var image = TestImages.WriteTiff(temp.Combine("albedo.tif"));

        // Act
        var first = sut.GetOrCreate(image);
        var second = sut.GetOrCreate(image);

        // Assert
        second.ShouldBe(first);
        File.Exists(first).ShouldBeTrue();
        converter.Verify(c => c.WriteThumbnail(image, It.IsAny<string>(), 128), Times.Once);
    }

    [Fact]
    public void GetOrCreate_ChangedImage_MakesANewPreview()
    {
        // Arrange
        var image = TestImages.WriteTiff(temp.Combine("albedo.tif"));
        var first = sut.GetOrCreate(image);
        File.SetLastWriteTimeUtc(image, File.GetLastWriteTimeUtc(image).AddMinutes(1));

        // Act
        var second = sut.GetOrCreate(image);

        // Assert
        second.ShouldNotBe(first);
    }

    [Fact]
    public void MagickImageConverter_WriteThumbnail_FitsTheSize()
    {
        // Arrange
        var image = TestImages.WriteTiff(temp.Combine("albedo.tif"), size: 512);
        var target = temp.Combine("thumb.png");

        // Act
        new MagickImageConverter().WriteThumbnail(image, target, 64);

        // Assert
        using var thumbnail = new MagickImage(target);
        thumbnail.Width.ShouldBe(64u);
        thumbnail.Format.ShouldBe(MagickFormat.Png);
    }
}
