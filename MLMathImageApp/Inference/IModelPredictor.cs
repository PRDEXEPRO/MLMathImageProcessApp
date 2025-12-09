namespace MLMathImageApp.Inference;

public interface IModelPredictor
{
    Task<ModelResult> PredictAsync(
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, 
        CancellationToken ct);
}


