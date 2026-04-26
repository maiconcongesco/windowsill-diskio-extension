using System.ComponentModel.Composition;
using System.Timers;
using WindowSill.API;

namespace WindowSill.DiskIo;

/// <summary>
/// Sill de Disk I/O: sempre visivel na barra, atualiza a cada 1 segundo.
/// Implementa ISillActivatedByDefault para ficar permanentemente ativo.
/// Implementa ISillSingleView para exibir view customizada (texto) na barra.
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
        _timer = new System.Timers.Timer(1_000);
        _timer.Elapsed += (_, _) =>
        {
            _latest = _collector.Sample();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    // Texto exibido na barra
    public string Title   => _latest.BarText;
    public string? Subtitle => _latest.TooltipText;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Dispose();
        _collector.Dispose();
    }
}
