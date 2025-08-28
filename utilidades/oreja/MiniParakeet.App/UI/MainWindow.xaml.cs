using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using MiniParakeet.Core.Audio;
using MiniParakeet.Core.Transcription;
using MiniParakeet.Core.Context;
using MiniParakeet.Core.Chat;
using System.Threading.Channels;
using System.Diagnostics;
using MiniParakeet.Core.Config;
using MiniParakeet.Core.Cost;
using MiniParakeet.Core.Export;
using MiniParakeet.Core.Updates;
using MiniParakeet.Core.LocalWhisper;
using System.Text;

namespace MiniParakeet.App.UI;

public partial class MainWindow : Window
{
    private readonly CancellationTokenSource _cts = new();
    private readonly IAudioCapturer _capturer = new WasapiLoopbackCapturer();
    // Aumentamos a 300ms para reducir número de fragmentos y carga sobre decodificador.
    private readonly AudioChunker _chunker = new(chunkMilliseconds:300);
    private readonly ContextRingBuffer _ring = new(120);
    private ITranscriptionProvider _transcription = new OpenAIWhisperProvider(new System.Net.Http.HttpClient(), string.Empty);
    private IChatProvider _chat = new OpenAIChatProvider(new System.Net.Http.HttpClient(), string.Empty);
    private readonly IChatProvider _offlineChat = new OfflineSummaryProvider();
    private readonly Stopwatch _summarySw = new();
    private Channel<byte[]> _raw = Channel.CreateUnbounded<byte[]>();
    private Channel<byte[]> _chunks = Channel.CreateUnbounded<byte[]>();
    private bool _paused;
    private readonly SettingsStore _settings = new();
    private readonly TokenCostEstimator _estimator = new();
    private readonly UpdateChecker _updateChecker = new(new System.Net.Http.HttpClient(), "https://example.com/version.json");
    private int _accumTokens;
    private decimal _accumCost;
    private bool _switchingProvider;
    // Acumulador completo de texto final para el siguiente resumen de bloque
    private readonly StringBuilder _fullTranscript = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        KeyDown += OnKeyDown;
        if (_capturer is WasapiLoopbackCapturer wc)
        {
            wc.LevelAvailable += (s, level) => Dispatcher.Invoke(() => UpdateVu(level));
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _settings.LoadAsync();
        // Cargar .env si existe para obtener API key (formato VARIABLE=valor)
        try
        {
            var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;
                    var key = line[..idx].Trim();
                    var val = line[(idx+1)..].Trim();
                    if (key.Equals("OPENAI_API_KEY", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(val))
                    {
                        if (string.IsNullOrWhiteSpace(_settings.GetApiKeyPlain())) _settings.SetApiKeyPlain(val);
                    }
                }
            }
        }
        catch { }
        // Fallback: variable de entorno
        var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? Environment.GetEnvironmentVariable("MINIPARAKEET_OPENAI_KEY");
        if (string.IsNullOrWhiteSpace(_settings.GetApiKeyPlain()) && !string.IsNullOrWhiteSpace(envKey)) _settings.SetApiKeyPlain(envKey);
    // Default: usar OpenAI (UseLocalWhisper=false). No tocamos flag aquí salvo migraciones futuras.
        ApiKeyBox.Password = _settings.GetApiKeyPlain() ?? string.Empty;
        PromptBox.Text = _settings.Current.PromptTemplate;
        MaxWordsBox.Text = _settings.Current.MaxWords.ToString();
        WindowSecondsBox.Text = _settings.Current.ContextWindowSeconds.ToString();
        EphemeralToggle.IsChecked = _settings.Current.EphemeralMode;
        _ = Task.Run(async () =>
        {
            var ver = await _updateChecker.GetLatestVersionAsync();
            if (ver != null)
                Dispatcher.Invoke(() => UpdateStatus($"Listo (Última versión: {ver})"));
        });
        var apiKey = _settings.GetApiKeyPlain();
        // Siempre creamos el proveedor de chat si hay key (aunque usemos whisper local para transcripción)
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _chat = new OpenAIChatProvider(new System.Net.Http.HttpClient(), apiKey);
        }
        // Si no usamos whisper local, también configuramos transcripción remota
        if (!_settings.Current.UseLocalWhisper && !string.IsNullOrWhiteSpace(apiKey))
        {
            _transcription = new OpenAIWhisperProvider(new System.Net.Http.HttpClient(), apiKey);
        }
        if (_settings.Current.UseLocalWhisper)
        {
            LocalWhisperToggle.IsChecked = true;
            SelectLocalModel(_settings.Current.LocalWhisperModel);
            _transcription = new LocalWhisperProvider(_settings.Current.LocalWhisperModel, 48000, 2);
        }
        else
        {
            LocalWhisperToggle.IsChecked = false; // OpenAI
            if (_transcription is not OpenAIWhisperProvider && !string.IsNullOrWhiteSpace(apiKey))
            {
                _transcription = new OpenAIWhisperProvider(new System.Net.Http.HttpClient(), apiKey);
            }
        }
        // Fallback: si no hay API key no podemos usar remoto
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _settings.Current.UseLocalWhisper = true;
            LocalWhisperToggle.IsChecked = true;
            SelectLocalModel(_settings.Current.LocalWhisperModel);
            _transcription = new LocalWhisperProvider(_settings.Current.LocalWhisperModel, 48000, 2);
            UpdateStatus("Sin API key: usando Whisper local");
        }
        AttachTranscriptionEvents(_transcription);
        _capturer.Start(_raw.Writer, _cts.Token);
        _ = Task.Run(() => _chunker.RunAsync(_raw.Reader, _chunks.Writer, _cts.Token));
        await _transcription.StartAsync(_cts.Token);
        await _transcription.PushChunkAsync(new byte[256], _cts.Token); // dummy
        _ = Task.Run(async () =>
        {
            await foreach (var ch in _chunks.Reader.ReadAllAsync(_cts.Token))
            {
                try { await _transcription.PushChunkAsync(ch, _cts.Token); } catch { }
            }
        });
    UpdateStatus("Listo");
    BasePathText.Text = System.IO.Path.GetFileName(AppContext.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar));
    }

    private void AddTranscript(string text, bool isFinal)
    {
        var tb = new TextBlock
        {
            Text = text,
            Style = (Style)FindResource(isFinal ? "FinalText" : "PartialText"),
            Margin = new Thickness(4,2,4,2)
        };
    // Nota: PartialText (gris) = hipótesis incremental de Whisper aún no confirmada; se reemplaza por líneas finales cuando IsFinal=true.
        TranscriptPanel.Children.Add(tb);
        ScrollToEnd();
    }

    private void UpdateCostDisplay()
    {
        CostText.Text = $"tokens={_accumTokens} coste=${_accumCost:F4}";
    }

    private void ScrollToEnd()
    {
        TranscriptScroll.ScrollToEnd();
    }

    private async Task RunSummaryAsync()
    {
        try
        {
            int maxWords = int.TryParse(MaxWordsBox.Text, out var m) ? m : 60;
            // Usar TODO el texto final capturado del bloque actual
            var all = _fullTranscript.ToString();
            if (string.IsNullOrWhiteSpace(all))
            {
                UpdateStatus("No hay texto para resumir");
                return;
            }
            // BuildSummaryPrompt espera un único bloque de contexto (string)
            var prompt = PromptBuilder.BuildSummaryPrompt(all, maxWords, PromptBox.Text);
            _summarySw.Restart();
            var result = await _chat.GenerateAsync(prompt, _cts.Token);
            if (result.StartsWith("[api error"))
            {
                var offline = await _offlineChat.GenerateAsync(prompt, _cts.Token);
                result += " | Offline: " + offline;
            }
            _summarySw.Stop();
            Dispatcher.Invoke(() =>
            {
                TranscriptPanel.Children.Clear();
                AddTranscript($"[Resumen {_summarySw.ElapsedMilliseconds}ms] {result}", true);
                _ring.Clear();
                _fullTranscript.Clear();
                _accumTokens = 0; _accumCost = 0; UpdateCostDisplay();
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => UpdateStatus("Error resumen: " + ex.Message));
        }
    }

    // Expuestos para Program / Tray
    public async Task RunSummaryAsyncWrapper() => await RunSummaryAsync();
    internal void TriggerPauseResume() => PauseResume_Click(this, new RoutedEventArgs());

    private void UpdateStatus(string msg) => StatusText.Text = msg;

    private void UpdateVu(double rms)
    {
        double db = 20 * Math.Log10(Math.Max(1e-6, rms));
        double norm = Math.Clamp((db + 60) / 60.0, 0, 1);
        VuFill.Width = 4 + norm * 116;
        VuFill.Fill = new SolidColorBrush(norm < 0.7 ? Colors.LimeGreen : (norm < 0.9 ? Colors.Orange : Colors.Red));
        VuText.Text = db.ToString("F0") + "dB";
    }

    private void PauseResume_Click(object sender, RoutedEventArgs e)
    {
        _paused = !_paused;
        if (_paused) { _capturer.Pause(); PauseResumeBtn.Content = "Reanudar"; UpdateStatus("Pausado"); }
        else { _capturer.Resume(); PauseResumeBtn.Content = "Pausa"; UpdateStatus("Capturando"); }
    }

    private void SelectLocalModel(string model)
    {
        foreach (var item in LocalModelCombo.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem ci && (string)ci.Content == model)
            {
                LocalModelCombo.SelectedItem = ci; break;
            }
        }
    }

    private async void LocalWhisperToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_switchingProvider) return;
        _switchingProvider = true;
        try
        {
            bool useLocal = LocalWhisperToggle.IsChecked == true;
            _settings.Current.UseLocalWhisper = useLocal;
            // Desuscribir y disponer proveedor anterior
            DetachTranscriptionEvents(_transcription);
            try { await _transcription.DisposeAsync(); } catch { }
            // Crear nuevo
            if (useLocal)
            {
                var model = (LocalModelCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "tiny";
                _transcription = new LocalWhisperProvider(model, 48000, 2);
                UpdateStatus("Inicializando whisper local...");
            }
            else
            {
                var key = _settings.GetApiKeyPlain() ?? string.Empty;
                _transcription = new OpenAIWhisperProvider(new System.Net.Http.HttpClient(), key);
                UpdateStatus("Inicializando proveedor remoto...");
            }
            AttachTranscriptionEvents(_transcription);
            await _transcription.StartAsync(_cts.Token);
            UpdateStatus(useLocal ? "Whisper local activo" : "Proveedor remoto OpenAI");
        }
        catch (Exception ex)
        {
            UpdateStatus("Error cambiando proveedor: " + ex.Message);
        }
        finally { _switchingProvider = false; }
    }

    private async void SummaryBtn_Click(object sender, RoutedEventArgs e) => await RunSummaryAsync();

    private async void FlushBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_transcription is MiniParakeet.Core.Transcription.OpenAIWhisperProvider openai)
        {
            UpdateStatus("Flush manual…");
            await openai.ForceFlushAsync(_cts.Token);
            UpdateStatus("Flush enviado");
        }
        else
        {
            UpdateStatus("Flush manual solo aplica a proveedor OpenAI");
        }
    }

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        TranscriptPanel.Children.Clear();
        _ring.Clear();
        UpdateStatus("Limpiado");
    _accumTokens = 0; _accumCost = 0; UpdateCostDisplay();
    }

    private async void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if ((System.Windows.Input.Keyboard.Modifiers & (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift)) == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift)
            && e.Key == System.Windows.Input.Key.R)
        {
            await RunSummaryAsync();
        }
    }

    private void ThemeToggle(object sender, RoutedEventArgs e)
    {
        bool dark = DarkModeToggle.IsChecked == true;
        var bg = (SolidColorBrush)FindResource("WindowBg");
    bg.Color = dark ? (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1e1e1e")! : System.Windows.Media.Colors.WhiteSmoke;
    }

    private async void SaveCfgBtn_Click(object sender, RoutedEventArgs e)
    {
    var oldKey = _settings.GetApiKeyPlain();
    _settings.SetApiKeyPlain(ApiKeyBox.Password);
        _settings.Current.PromptTemplate = PromptBox.Text;
        if (int.TryParse(MaxWordsBox.Text, out var mw)) _settings.Current.MaxWords = mw;
        if (int.TryParse(WindowSecondsBox.Text, out var ws)) _settings.Current.ContextWindowSeconds = ws;
        _settings.Current.EphemeralMode = EphemeralToggle.IsChecked == true;
        try { if (!_settings.Current.EphemeralMode) await _settings.SaveAsync(); UpdateStatus("Guardado"); }
        catch (Exception ex) { UpdateStatus("Error guardando: " + ex.Message); }
        var newKey = _settings.GetApiKeyPlain();
        if (newKey != oldKey && !string.IsNullOrWhiteSpace(newKey))
        {
            _chat = new OpenAIChatProvider(new System.Net.Http.HttpClient(), newKey);
        }
    }

    private async void ExportBtn_Click(object sender, RoutedEventArgs e)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var child in TranscriptPanel.Children)
        {
            if (child is TextBlock tb) sb.AppendLine(tb.Text);
        }
        try
        {
            var path = await TranscriptExporter.ExportAsync(sb.ToString());
            UpdateStatus("Exportado: " + System.IO.Path.GetFileName(path));
        }
        catch (Exception ex) { UpdateStatus("Error exportar: " + ex.Message); }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _cts.Cancel();
        _transcription.DisposeAsync().AsTask().Wait(200);
    // Asegura que no queden hilos en segundo plano reteniendo el DLL durante rebuild.
    Environment.Exit(0);
    }

    private void AttachTranscriptionEvents(ITranscriptionProvider provider)
    {
        provider.TranscriptionUpdated += OnTranscriptionUpdated;
    }

    private void DetachTranscriptionEvents(ITranscriptionProvider provider)
    {
        provider.TranscriptionUpdated -= OnTranscriptionUpdated;
    }

    private void OnTranscriptionUpdated(object? sender, TranscriptionUpdateEventArgs ev)
    {
        Dispatcher.Invoke(() => AddTranscript(ev.Text, ev.IsFinal));
        if (ev.IsFinal)
        {
            _ring.Add(ev.Text);
            var (tokens, cost) = _estimator.Estimate(ev.Text);
            _accumTokens += tokens;
            _accumCost += cost;
            Dispatcher.Invoke(UpdateCostDisplay);
            _fullTranscript.AppendLine(ev.Text);
        }
    }
}
