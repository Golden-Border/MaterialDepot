namespace MaterialDepot.Core.Downloads;

/// <summary>
/// Takes a file out of the download folder.
/// </summary>
public interface IFileRemover
{
    void Remove(string path);
}
