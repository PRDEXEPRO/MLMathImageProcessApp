using MLMathImageApp.Config;
using MLMathImageApp.Inference;
using MLMathImageApp.Utils;

namespace MLMathImageApp.Processing;

public sealed class ImagePipeline
{
    private readonly IReadOnlyList<IImageProcessor> _processors;
    private readonly IModelPredictor _predictor;
    private readonly ImageIo _io;

    public ImagePipeline(IEnumerable<IImageProcessor> processors, IModelPredictor predictor, ImageIo io)
    {
        _processors = processors.ToList();
        _predictor = predictor;
        _io = io;
    }

    public async Task RunAsync(AppConfig config, CancellationToken ct)
    {
        Directory.CreateDirectory(config.InputDir);
        Directory.CreateDirectory(config.OutputDir);

        var files = await _io.DiscoverImagesAsync(config.InputDir, ct);
        if (files.Count == 0)
        {
            Console.WriteLine("Girdi klasöründe işlenecek görüntü bulunamadı.");
            return;
        }

        var selected = files.Take(config.MaxImages).ToList();
        Console.WriteLine($"Toplam {selected.Count} görsel işlenecek...");

        foreach (var file in selected)
        {
            ct.ThrowIfCancellationRequested();
            await ProcessFileAsync(file, config.OutputDir, ct);
        }
    }

    private async Task ProcessFileAsync(string inputPath, string outputDir, CancellationToken ct)
    {
        using var image = await _io.LoadAsync(inputPath, ct);
        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> current = image;

        foreach (var processor in _processors)
        {
            current = await processor.ProcessAsync(current, ct);
        }

        var result = await _predictor.PredictAsync(current, ct);
        var fileName = Path.GetFileNameWithoutExtension(inputPath);
        var outPath = Path.Combine(outputDir, $"{fileName}_processed.png");
        await _io.SaveAsync(current, outPath, ct);

        Console.WriteLine($"{fileName}: skor={result.Score:F3}, etiket={result.Label}");
    }
}


