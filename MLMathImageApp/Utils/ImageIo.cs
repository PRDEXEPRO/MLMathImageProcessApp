using SixLabors.ImageSharp.Formats.Png;

namespace MLMathImageApp.Utils;

public sealed class ImageIo
{
    public async Task<List<string>> DiscoverImagesAsync(string directory, CancellationToken ct)
    {
        if (!Directory.Exists(directory))
        {
            return new List<string>();
        }

        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        return await Task.Run(() =>
            Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => extensions.Contains(Path.GetExtension(f)))
                .ToList(), ct);
    }

    public async Task<SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>> LoadAsync(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        return await SixLabors.ImageSharp.Image.LoadAsync<SixLabors.ImageSharp.PixelFormats.Rgba32>(stream, ct);
    }

    public async Task SaveAsync(SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, string path, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = File.Create(path);
        await image.SaveAsync(stream, new PngEncoder(), ct);
    }
}


