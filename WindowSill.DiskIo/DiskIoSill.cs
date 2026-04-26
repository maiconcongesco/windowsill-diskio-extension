using System.ComponentModel.Composition;
using System.Timers;
using WindowSill.Sdk;
using WindowSill.Sdk.Activation;
using WindowSill.Sdk.Views;

namespace WindowSill.DiskIo;

/// <summary>
/// Sill principal: exibe metricas de disco em tempo real na barra do WindowSill.
/// Atualiza a cada 1 segundo via timer.
/// </summary>
[Export(typeof(ISill))]
[Name("Disk I/O")]
public sealed class DiskIoSill : ISillActivatedByDefault, ISillSingleView, IDisposable
{
    private readonly DiskIoCollector _collector = new();
    private readonly System.Timers.Timer _timer;
    private DiskIoSnapshot _latest = new(0, 0, 0);
    private bool _disposed;

    public event EventHandler? ContentChanged;

    public DiskIoSill()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTick;
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void OnTick(object? sender, ElapsedEventArgs e)
    {
        _latest = _collector.Sample();
        ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Texto exibido na barra do WindowSill.</summary>
    public string Title => _latest.Format();

    /// <summary>Tooltip com detalhes ao passar o mouse.</summary>
    public string? Subtitle =>
        $"Leitura:  {_latest.ReadMbps:F2} MB/s\n" +
        $"Escrita:  {_latest.WriteMbps:F2} MB/s\n" +
        $"Fila:     {_latest.QueueLength} req";

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Dispose();
        _collector.Dispose();
    }
}
