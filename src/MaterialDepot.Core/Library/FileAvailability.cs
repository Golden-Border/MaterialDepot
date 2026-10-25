namespace MaterialDepot.Core.Library;

/// <summary>
/// Whether a file's content is on this disk or only in a sync client's cloud, such as a OneDrive placeholder.
/// </summary>
public static class FileAvailability
{
    // FILE_ATTRIBUTE_RECALL_ON_OPEN and FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS; .NET has no names for them.
    private const FileAttributes RecallOnOpen = (FileAttributes)0x40000;
    private const FileAttributes RecallOnDataAccess = (FileAttributes)0x400000;

    /// <summary>
    /// <see langword="true"/> for a placeholder whose content the sync client would have to download first. Reading
    /// one starts that download, which can fail, so a preview of the whole library must not do it.
    /// </summary>
    public static bool IsCloudOnly(string path)
    {
        try
        {
            var attributes = File.GetAttributes(path);
            return (attributes & (FileAttributes.Offline | RecallOnOpen | RecallOnDataAccess)) != 0;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
