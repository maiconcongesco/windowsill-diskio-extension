using System.Composition;
using System.Timers;
using WindowSill.API;

namespace WindowSill.DiskIo;

/// <summary>
/// Sill de Disk I/O: atualiza a cada 1 segundo, exibe R/W MB/s na barra.
/// </summary>
[Export(typeof(ISill))]
[Name("Disk I/O")]
public sealed class DiskIoSill : ISill, ISillSingleView, IDisposable
{
    private readonly DiskIoCollector _collector = new();
    private readonly System.Timers.Timer _timer;
    private DiskIoSnapshot _latest = new(0, 0, 0);
    private bool _disposed;

    public event EventHandler? ContentChanged;

    // --- ISill ---
    public string DisplayName => "Disk I/O";
    public SillSettingsView[] SettingsViews => Array.Empty<SillSettingsView>();
    public ValueTask OnActivatedAsync()   => ValueTask.CompletedTask;
    public ValueTask OnDeactivatedAsync() => ValueTask.CompletedTask;

    // --- ISillSingleView ---
    public SillView View => new SillView
    {
        Title    = _latest.BarText,
        Subtitle = _latest.TooltipText
    };

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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Dispose();
        _collector.Dispose();
    }
}
