namespace MaterialDepot.Core.Setup;

/// <summary>
/// What a folder path the user typed or picked turns out to be.
/// </summary>
public enum FolderStatus
{
    /// <summary>The folder exists.</summary>
    Exists,

    /// <summary>A valid absolute path, but there is no such folder (yet).</summary>
    Missing,

    /// <summary>Nothing was entered.</summary>
    Empty,

    /// <summary>Not an absolute path, or one with characters a path cannot have.</summary>
    Invalid,

    /// <summary>The path names a file.</summary>
    IsFile,
}
