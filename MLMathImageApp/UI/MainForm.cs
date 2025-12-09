using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using MLMathImageApp.Config;
using MLMathImageApp.Inference;
using MLMathImageApp.Processing;
using MLMathImageApp.Utils;

namespace MLMathImageApp.UI;

/// <summary>
/// Ana Windows Forms arayüzü: görüntü seçimi, işleme ve sonuç gösterimi.
/// </summary>
public sealed class MainForm : Form
{
    private readonly TextBox _inputDirTextBox;
    private readonly TextBox _outputDirTextBox;
    private readonly TextBox _modelPathTextBox;
    private readonly NumericUpDown _maxImagesNumeric;
    private readonly Button _selectInputBtn;
    private readonly Button _selectOutputBtn;
    private readonly Button _selectModelBtn;
    private readonly Button _processBtn;
    private readonly RichTextBox _logTextBox;
    private readonly ProgressBar _progressBar;
    private readonly Label _statusLabel;
    private readonly PictureBox _previewPictureBox;
    private CancellationTokenSource? _cancellationTokenSource;

    public MainForm()
    {
        Text = "ML Math Image Processor";
        Size = new System.Drawing.Size(1000, 700);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

        // Başlık
        var titleLabel = new Label
        {
            Text = "ML Math Image Processor",
            Font = new System.Drawing.Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(30, 30, 30),
            AutoSize = true,
            Location = new System.Drawing.Point(20, 15)
        };
        Controls.Add(titleLabel);

        // Girdi klasörü
        var inputLabel = new Label
        {
            Text = "Girdi Klasörü:",
            Font = new System.Drawing.Font("Segoe UI", 10),
            Location = new System.Drawing.Point(20, 60),
            AutoSize = true
        };
        Controls.Add(inputLabel);

        _inputDirTextBox = new TextBox
        {
            Location = new System.Drawing.Point(20, 85),
            Size = new System.Drawing.Size(600, 25),
            Font = new System.Drawing.Font("Segoe UI", 9),
            Text = Path.Combine(Environment.CurrentDirectory, "data", "input")
        };
        Controls.Add(_inputDirTextBox);

        _selectInputBtn = new Button
        {
            Text = "Klasör Seç",
            Location = new System.Drawing.Point(630, 83),
            Size = new System.Drawing.Size(100, 30),
            Font = new System.Drawing.Font("Segoe UI", 9),
            BackColor = System.Drawing.Color.FromArgb(70, 130, 180),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _selectInputBtn.FlatAppearance.BorderSize = 0;
        _selectInputBtn.Click += (s, e) => SelectFolder(_inputDirTextBox);
        Controls.Add(_selectInputBtn);

        // Çıktı klasörü
        var outputLabel = new Label
        {
            Text = "Çıktı Klasörü:",
            Font = new System.Drawing.Font("Segoe UI", 10),
            Location = new System.Drawing.Point(20, 125),
            AutoSize = true
        };
        Controls.Add(outputLabel);

        _outputDirTextBox = new TextBox
        {
            Location = new System.Drawing.Point(20, 150),
            Size = new System.Drawing.Size(600, 25),
            Font = new System.Drawing.Font("Segoe UI", 9),
            Text = Path.Combine(Environment.CurrentDirectory, "data", "output")
        };
        Controls.Add(_outputDirTextBox);

        _selectOutputBtn = new Button
        {
            Text = "Klasör Seç",
            Location = new System.Drawing.Point(630, 148),
            Size = new System.Drawing.Size(100, 30),
            Font = new System.Drawing.Font("Segoe UI", 9),
            BackColor = System.Drawing.Color.FromArgb(70, 130, 180),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _selectOutputBtn.FlatAppearance.BorderSize = 0;
        _selectOutputBtn.Click += (s, e) => SelectFolder(_outputDirTextBox);
        Controls.Add(_selectOutputBtn);

        // Model yolu (opsiyonel)
        var modelLabel = new Label
        {
            Text = "Model Yolu (Opsiyonel):",
            Font = new System.Drawing.Font("Segoe UI", 10),
            Location = new System.Drawing.Point(20, 190),
            AutoSize = true
        };
        Controls.Add(modelLabel);

        _modelPathTextBox = new TextBox
        {
            Location = new System.Drawing.Point(20, 215),
            Size = new System.Drawing.Size(600, 25),
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        Controls.Add(_modelPathTextBox);

        _selectModelBtn = new Button
        {
            Text = "Dosya Seç",
            Location = new System.Drawing.Point(630, 213),
            Size = new System.Drawing.Size(100, 30),
            Font = new System.Drawing.Font("Segoe UI", 9),
            BackColor = System.Drawing.Color.FromArgb(70, 130, 180),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _selectModelBtn.FlatAppearance.BorderSize = 0;
        _selectModelBtn.Click += (s, e) => SelectModelFile();
        Controls.Add(_selectModelBtn);

        // Maksimum görsel sayısı
        var maxImagesLabel = new Label
        {
            Text = "Maksimum İşlenecek Görsel Sayısı:",
            Font = new System.Drawing.Font("Segoe UI", 10),
            Location = new System.Drawing.Point(20, 255),
            AutoSize = true
        };
        Controls.Add(maxImagesLabel);

        _maxImagesNumeric = new NumericUpDown
        {
            Location = new System.Drawing.Point(20, 280),
            Size = new System.Drawing.Size(120, 25),
            Font = new System.Drawing.Font("Segoe UI", 9),
            Minimum = 1,
            Maximum = 1000,
            Value = 32
        };
        Controls.Add(_maxImagesNumeric);

        // İşleme butonu
        _processBtn = new Button
        {
            Text = "İşlemeyi Başlat",
            Location = new System.Drawing.Point(750, 83),
            Size = new System.Drawing.Size(200, 50),
            Font = new System.Drawing.Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = System.Drawing.Color.FromArgb(34, 139, 34),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _processBtn.FlatAppearance.BorderSize = 0;
        _processBtn.Click += ProcessBtn_Click;
        Controls.Add(_processBtn);

        // İptal butonu
        var cancelBtn = new Button
        {
            Text = "İptal",
            Location = new System.Drawing.Point(750, 143),
            Size = new System.Drawing.Size(200, 30),
            Font = new System.Drawing.Font("Segoe UI", 9),
            BackColor = System.Drawing.Color.FromArgb(220, 20, 60),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        cancelBtn.FlatAppearance.BorderSize = 0;
        cancelBtn.Click += (s, e) =>
        {
            _cancellationTokenSource?.Cancel();
            cancelBtn.Enabled = false;
            _processBtn.Enabled = true;
        };
        Controls.Add(cancelBtn);

        // Progress bar
        _progressBar = new ProgressBar
        {
            Location = new System.Drawing.Point(20, 320),
            Size = new System.Drawing.Size(930, 25),
            Style = ProgressBarStyle.Continuous
        };
        Controls.Add(_progressBar);

        // Durum etiketi
        _statusLabel = new Label
        {
            Text = "Hazır",
            Font = new System.Drawing.Font("Segoe UI", 9),
            Location = new System.Drawing.Point(20, 355),
            AutoSize = true,
            ForeColor = System.Drawing.Color.FromArgb(50, 50, 50)
        };
        Controls.Add(_statusLabel);

        // Log alanı
        var logLabel = new Label
        {
            Text = "İşlem Logları:",
            Font = new System.Drawing.Font("Segoe UI", 10),
            Location = new System.Drawing.Point(20, 385),
            AutoSize = true
        };
        Controls.Add(logLabel);

        _logTextBox = new RichTextBox
        {
            Location = new System.Drawing.Point(20, 410),
            Size = new System.Drawing.Size(600, 240),
            Font = new System.Drawing.Font("Consolas", 9),
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
            ForeColor = System.Drawing.Color.FromArgb(200, 200, 200),
            ReadOnly = true
        };
        Controls.Add(_logTextBox);

        // Önizleme alanı
        var previewLabel = new Label
        {
            Text = "Görsel Önizleme:",
            Font = new System.Drawing.Font("Segoe UI", 10),
            Location = new System.Drawing.Point(640, 385),
            AutoSize = true
        };
        Controls.Add(previewLabel);

        _previewPictureBox = new PictureBox
        {
            Location = new System.Drawing.Point(640, 410),
            Size = new System.Drawing.Size(310, 240),
            BackColor = System.Drawing.Color.FromArgb(50, 50, 50),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(_previewPictureBox);

        Log("Uygulama başlatıldı. Girdi klasörünü seçip işlemeyi başlatabilirsiniz.");
    }

    private void SelectFolder(TextBox textBox)
    {
        try
        {
            var initialPath = textBox.Text;
            if (!string.IsNullOrWhiteSpace(initialPath) && !Directory.Exists(initialPath))
            {
                initialPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else if (string.IsNullOrWhiteSpace(initialPath))
            {
                initialPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            var selectedPath = FolderBrowserHelper.BrowseForFolder(this, "Klasör seçin", initialPath);
            
            if (!string.IsNullOrWhiteSpace(selectedPath) && Directory.Exists(selectedPath))
            {
                textBox.Text = selectedPath;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Klasör seçilirken hata oluştu:\n{ex.Message}", "Hata", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SelectModelFile()
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Model dosyası seçin",
                Filter = "Model dosyaları|*.onnx;*.pb;*.tflite;*.mlnet|Tüm dosyalar|*.*",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                _modelPathTextBox.Text = dialog.FileName;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Dosya seçilirken hata oluştu:\n{ex.Message}", "Hata", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string>(Log), message);
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logTextBox.AppendText($"[{timestamp}] {message}\n");
        _logTextBox.SelectionStart = _logTextBox.Text.Length;
        _logTextBox.ScrollToCaret();
    }

    private void UpdateStatus(string status)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string>(UpdateStatus), status);
            return;
        }

        _statusLabel.Text = status;
    }

    private void UpdateProgress(int value, int maximum)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<int, int>(UpdateProgress), value, maximum);
            return;
        }

        _progressBar.Maximum = maximum;
        _progressBar.Value = Math.Min(value, maximum);
    }

    private async void ProcessBtn_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_inputDirTextBox.Text) || !Directory.Exists(_inputDirTextBox.Text))
        {
            MessageBox.Show("Lütfen geçerli bir girdi klasörü seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(_outputDirTextBox.Text))
        {
            MessageBox.Show("Lütfen bir çıktı klasörü belirtin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _processBtn.Enabled = false;
        _cancellationTokenSource = new CancellationTokenSource();
        var ct = _cancellationTokenSource.Token;

        try
        {
            var config = new AppConfig(
                _inputDirTextBox.Text,
                _outputDirTextBox.Text,
                string.IsNullOrWhiteSpace(_modelPathTextBox.Text) ? null : _modelPathTextBox.Text,
                (int)_maxImagesNumeric.Value
            );

            var processors = new IImageProcessor[]
            {
                new GrayscaleProcessor(),
                new NormalizeProcessor(),
                new EdgeDetectProcessor()
            };

            IModelPredictor predictor = new MockModelPredictor();
            var pipeline = new ImagePipeline(processors, predictor, new ImageIo());

            Log($"İşleme başlatılıyor... Girdi: {config.InputDir}, Çıktı: {config.OutputDir}");
            UpdateStatus("İşleniyor...");

            await Task.Run(async () =>
            {
                Directory.CreateDirectory(config.InputDir);
                Directory.CreateDirectory(config.OutputDir);

                var io = new ImageIo();
                var files = await io.DiscoverImagesAsync(config.InputDir, ct);
                if (files.Count == 0)
                {
                    Log("Girdi klasöründe işlenecek görüntü bulunamadı.");
                    UpdateStatus("Görüntü bulunamadı");
                    return;
                }

                var selected = files.Take(config.MaxImages).ToList();
                Log($"Toplam {selected.Count} görsel işlenecek...");
                UpdateProgress(0, selected.Count);

                for (int i = 0; i < selected.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var file = selected[i];

                    try
                    {
                        using var image = await io.LoadAsync(file, ct);
                        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> current = image;

                        foreach (var processor in processors)
                        {
                            current = await processor.ProcessAsync(current, ct);
                        }

                        var result = await predictor.PredictAsync(current, ct);
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var outPath = Path.Combine(config.OutputDir, $"{fileName}_processed.png");
                        await io.SaveAsync(current, outPath, ct);

                        Log($"{fileName}: skor={result.Score:F3}, etiket={result.Label}");
                        UpdateProgress(i + 1, selected.Count);

                        // İlk görseli önizleme olarak göster
                        if (i == 0)
                        {
                            Invoke(new Action(() =>
                            {
                                using var ms = new MemoryStream();
                                current.SaveAsPng(ms);
                                ms.Position = 0;
                                _previewPictureBox.Image?.Dispose();
                                _previewPictureBox.Image = System.Drawing.Image.FromStream(ms);
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Hata ({Path.GetFileName(file)}): {ex.Message}");
                    }
                }

                Log("İşlem tamamlandı!");
                UpdateStatus("Tamamlandı");
            }, ct);
        }
        catch (OperationCanceledException)
        {
            Log("İşlem kullanıcı tarafından iptal edildi.");
            UpdateStatus("İptal edildi");
        }
        catch (Exception ex)
        {
            Log($"Kritik hata: {ex.Message}");
            UpdateStatus("Hata oluştu");
            MessageBox.Show($"İşlem sırasında hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _processBtn.Enabled = true;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _previewPictureBox.Image?.Dispose();
        base.OnFormClosing(e);
    }
}

