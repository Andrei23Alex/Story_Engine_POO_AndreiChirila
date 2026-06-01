using System.IO;
using System.Collections.Generic;

namespace Story.Persistence;

public class ImageRepository : IDisposable
{
    private readonly string _basePath;
    // Specificăm clar System.Drawing.Image ca să nu mai existe confuzii
    private readonly Dictionary<string, System.Drawing.Image> _cache = new();

    public ImageRepository(string basePath)
    {
        _basePath = basePath;
    }

    public System.Drawing.Image? GetImage(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;

        if (_cache.TryGetValue(fileName, out var cachedImg))
        {
            return cachedImg;
        }

        string fullPath = Path.Combine(_basePath, "images", fileName);
        if (!File.Exists(fullPath)) return null;

        try
        {
            // Folosim Bitmap din System.Drawing ca să înrcăm imaginea corect
            var img = new System.Drawing.Bitmap(fullPath);
            _cache[fileName] = img;
            return img;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        foreach (var img in _cache.Values)
        {
            img.Dispose();
        }
        _cache.Clear();
    }
}