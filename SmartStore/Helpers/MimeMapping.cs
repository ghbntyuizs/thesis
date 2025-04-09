using System.IO;

namespace SmartStorePOS.Helpers
{
    public static class MimeMapping
    {
        private static readonly Dictionary<string, string> MimeTypes = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".bmp", "image/bmp" },
        { ".tiff", "image/tiff" },
        { ".webp", "image/webp" },
        { ".svg", "image/svg+xml" },
        { ".pdf", "application/pdf" },
        { ".txt", "text/plain" }
    };

        public static string GetMimeType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return MimeTypes.TryGetValue(ext, out var mimeType) ? mimeType : "application/octet-stream";
        }
    }
}
