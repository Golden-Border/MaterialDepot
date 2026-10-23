using System.Runtime.Versioning;
using Microsoft.VisualBasic.FileIO;

namespace MaterialDepot.Core.Downloads;

/// <summary>
/// Sends files to the Windows recycle bin, so a zip moved away with more in it than the maps can still be got back.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RecycleBinFileRemover : IFileRemover
{
    public void Remove(string path) => FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
}
