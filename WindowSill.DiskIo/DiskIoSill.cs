using System.ComponentModel.Composition;
using System.Timers;
using WindowSill.API;

namespace WindowSill.DiskIo;

/// <summary>
/// Sill principal: aparece sempre na barra e atualiza metricas de disco a cada 1 segundo.
/// </summary>
[Export(typeof(ISill))]
public sealed class DiskIoSill : ISill, IDisposable
{
    private readonly DiskIoCollector _collector = new();
    private readonly System.Timers.Timer _timer;
    private DiskIoSnapshot _latest = new(0, 0, 0);
    private bool _disposed;

    public event EventHandler? Invalidated;

    public DiskIoSill()
    {
        _timer = new System.Timers.Timer(1_000);
        _timer.Elapsed += (_, _) =>
        {
            _latest = _collector.Sample();
            Invalidated?.Invoke(this, EventArgs.Empty);
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    /// <summary>Texto exibido na barra do WindowSill.</summary>
    public string Label => _latest.Format();

    /// <summary>Tooltip ao passar o mouse.</summary>
    public string? Tooltip =>
        $"Leitura: {_latest.ReadMbps:F2} MB/s\n" +
        $"Escrita: {_latest.WriteMbps:F2} MB/s\n" +
        $"Fila:    {_latest.QueueLength} req";

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Dispose();
        _collector.Dispose();
    }
}
