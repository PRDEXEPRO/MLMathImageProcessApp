using SixLabors.ImageSharp.Processing;

namespace MLMathImageApp.Processing;

public sealed class GrayscaleProcessor : IImageProcessor
{
    public string Name => "grayscale";

    public Task<SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>> ProcessAsync(
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, CancellationToken ct)
    {
        image.Mutate(ctx => ctx.Grayscale());
        return Task.FromResult(image);
    }
}


