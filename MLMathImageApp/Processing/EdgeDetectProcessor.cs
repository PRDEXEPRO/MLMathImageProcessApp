using SixLabors.ImageSharp.Processing;

namespace MLMathImageApp.Processing;

public sealed class EdgeDetectProcessor : IImageProcessor
{
    public string Name => "edges";

    public Task<SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>> ProcessAsync(
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, CancellationToken ct)
    {
        image.Mutate(ctx => ctx.DetectEdges());
        return Task.FromResult(image);
    }
}


