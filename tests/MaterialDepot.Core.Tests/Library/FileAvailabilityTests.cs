using MaterialDepot.Core.Library;

namespace MaterialDepot.Core.Tests.Library;

public sealed class FileAvailabilityTests : IDisposable
{
    private readonly TempDirectory temp = new();

    public void Dispose() => temp.Dispose();

    [Fact]
    public void IsCloudOnly_LocalFile_IsFalse()
    {
        // Arrange
        var path = temp.Combine("albedo.tif");
        File.WriteAllBytes(path, [1]);

        // Act & Assert
        FileAvailability.IsCloudOnly(path).ShouldBeFalse();
    }

    [Fact]
    public void IsCloudOnly_FileMarkedOffline_IsTrue()
    {
        // Arrange
        var path = temp.Combine("albedo.tif");
        File.WriteAllBytes(path, [1]);
        File.SetAttributes(path, FileAttributes.Offline);

        // Act & Assert
        FileAvailability.IsCloudOnly(path).ShouldBeTrue();
    }

    [Fact]
    public void IsCloudOnly_MissingFile_IsFalse()
    {
        // Act & Assert
        FileAvailability.IsCloudOnly(temp.Combine("missing.tif")).ShouldBeFalse();
    }
}
