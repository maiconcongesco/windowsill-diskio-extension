using System.Composition;
using System.Timers;
using Microsoft.UI.Xaml.Controls;
using WindowSill.API;

namespace WindowSill.DiskIo;

/// <summary>
/// Sill de Disk I/O: sempre visivel na barra, atualiza a cada 1 segundo.
/// Implementa ISillSingleView para exibir texto customizado na barra.
/// </summary>
[Export(typeof(ISill))]
[Name("Disk I/O")]
public sealed class DiskIoSill : ISill, ISillSingleView, ISillActivatedByDefault, IDisposable
{
    private readonly DiskIoCollector _collector = new();
    private readonly System.Timers.Timer _timer;
    private DiskIoSnapshot _latest = new(0, 0, 0);
    private bool _disposed;

    public event EventHandler? ContentChanged;

    // --- ISill ---
    public string DisplayName  => "Disk I/O";
    public IReadOnlyList<object> SettingsViews => Array.Empty<object>();

    public Task OnActivatedAsync()   => Task.CompletedTask;
    public Task OnDeactivatedAsync() => Task.CompletedTask;

    // --- ISillSingleView ---
    public object View => BuildView();

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

    private object BuildView()
    {
        return new TextBlock
        {
            Text    = _latest.BarText,
            ToolTipService = { ToolTip = _latest.TooltipText }
        };
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
