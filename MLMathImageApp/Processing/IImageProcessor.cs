namespace MLMathImageApp.Processing;

public interface IImageProcessor
{
    string Name { get; }
    Task<SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>> ProcessAsync(
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, 
        CancellationToken ct);
}


