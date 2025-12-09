using SixLabors.ImageSharp.Processing;

namespace MLMathImageApp.Processing;

/// <summary>
/// Parlaklık/kontrastı normalize eder ve değerleri 0-1 aralığına sıkıştırır.
/// </summary>
public sealed class NormalizeProcessor : IImageProcessor
{
    public string Name => "normalize";

    public Task<SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>> ProcessAsync(
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, CancellationToken ct)
    {
        image.Mutate(ctx =>
        {
            ctx.Brightness(1.05f);
            ctx.Contrast(1.1f);
        });
        return Task.FromResult(image);
    }
}


