using System.IO.Compression;
using System.Security.Cryptography;

namespace MaterialDepot.Core.Downloads;

public sealed class DownloadArchiver(IFileRemover fileRemover) : IDownloadArchiver
{
    public ArchiveResult Archive(DownloadCandidate candidate, string setName, string libraryRoot, bool move)
    {
        if (string.IsNullOrWhiteSpace(setName) || setName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException($"'{setName}' is no folder name.", nameof(setName));
        }

        var setDirectory = Path.Combine(libraryRoot, setName.Trim());
        Directory.CreateDirectory(setDirectory);

        List<string> copied = [];
        List<string> alreadyThere = [];
        List<string> conflicts = [];
        foreach (var file in candidate.Files)
        {
            var target = Path.Combine(setDirectory, file.FileName);
            if (File.Exists(target))
            {
                (SameContent(file, target) ? alreadyThere : conflicts).Add(target);
                continue;
            }

            Copy(file, target);
            copied.Add(target);
        }

        List<string> removed = [];
        if (move && conflicts.Count == 0)
        {
            foreach (var source in candidate.SourceFiles)
            {
                fileRemover.Remove(source);
                removed.Add(source);
            }
        }

        return new ArchiveResult(setDirectory, copied, alreadyThere, conflicts, removed);
    }

    private static void Copy(DownloadedMap file, string target)
    {
        if (file.ZipEntry is null)
        {
            File.Copy(file.Location, target);
            return;
        }

        using var zip = ZipFile.OpenRead(file.Location);
        zip.GetEntry(file.ZipEntry)!.ExtractToFile(target);
    }

    private static bool SameContent(DownloadedMap file, string target)
    {
        using var existing = File.OpenRead(target);
        if (existing.Length != file.Length)
        {
            return false;
        }

        var existingHash = SHA256.HashData(existing);
        if (file.ZipEntry is null)
        {
            using var source = File.OpenRead(file.Location);
            return existingHash.AsSpan().SequenceEqual(SHA256.HashData(source));
        }

        using var zip = ZipFile.OpenRead(file.Location);
        using var entry = zip.GetEntry(file.ZipEntry)!.Open();
        return existingHash.AsSpan().SequenceEqual(SHA256.HashData(entry));
    }
}
