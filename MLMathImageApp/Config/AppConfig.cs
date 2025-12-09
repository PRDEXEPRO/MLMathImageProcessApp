namespace MLMathImageApp.Config;

/// <summary>
/// Temel uygulama yapılandırması; CLI argümanlarından veya varsayılandan üretilir.
/// </summary>
public sealed record AppConfig(string InputDir, string OutputDir, string? ModelPath, int MaxImages)
{
    public static AppConfig Parse(string[] args)
    {
        // Basit argüman sözlüğü: --girdi=, --cikti=, --model=, --limit=
        var map = args
            .Where(a => a.StartsWith("--"))
            .Select(a =>
            {
                var parts = a.Split('=', 2);
                return (key: parts[0].TrimStart('-'), value: parts.Length > 1 ? parts[1] : string.Empty);
            })
            .ToDictionary(k => k.key, v => v.value, StringComparer.OrdinalIgnoreCase);

        var input = map.TryGetValue("girdi", out var i) && !string.IsNullOrWhiteSpace(i)
            ? i
            : Path.Combine(Environment.CurrentDirectory, "data", "input");

        var output = map.TryGetValue("cikti", out var o) && !string.IsNullOrWhiteSpace(o)
            ? o
            : Path.Combine(Environment.CurrentDirectory, "data", "output");

        var model = map.TryGetValue("model", out var m) && !string.IsNullOrWhiteSpace(m) ? m : null;

        var limit = map.TryGetValue("limit", out var l) && int.TryParse(l, out var parsed) && parsed > 0
            ? parsed
            : 32;

        return new AppConfig(input, output, model, limit);
    }
}




