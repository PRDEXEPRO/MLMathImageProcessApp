namespace MLMathImageApp.Inference;

/// <summary>
/// Gerçek model entegrasyonu öncesi basit sahte tahminleyici.
/// </summary>
public sealed class MockModelPredictor : IModelPredictor
{
    private static readonly string[] Labels = { "kenar", "doku", "parlak", "karma" };
    private readonly Random _random = new();

    public Task<ModelResult> PredictAsync(
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, CancellationToken ct)
    {
        // Görüntü boyutundan basit bir özellik çıkarımı simülasyonu.
        var feature = (image.Width + image.Height) % Labels.Length;
        var label = Labels[feature];
        var score = _random.NextDouble();

        return Task.FromResult(new ModelResult(label, score));
    }
}


