using System.IO.Compression;

namespace GodotFest.VisaLetter.Services;

/// <summary>
/// Bundles the generated files into a single archive.
/// Browsers block the second and subsequent automatic downloads from one user gesture, so
/// when both languages are requested the files are delivered as one zip instead.
/// </summary>
public static class ZipBuilder
{
    public static string ToBase64(IEnumerable<(string Name, byte[] Content)> entries)
    {
        using var buffer = new MemoryStream();

        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
                using var stream = entry.Open();
                stream.Write(content, 0, content.Length);
            }
        }

        return Convert.ToBase64String(buffer.ToArray());
    }
}
