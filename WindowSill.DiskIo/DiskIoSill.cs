using System;
using System.Composition;
using Microsoft.UI.Xaml.Controls;
using WindowSill.API;

namespace WindowSill.DiskIo;

/// <summary>
/// Sill de Disk I/O: ativo por padrão, atualiza a cada 1 segundo.
/// Exibe leitura/escrita (MB/s) na barra do WindowSill.
/// </summary>
[Export(typeof(ISillActivatedByDefault))]
[Name("Disk I/O")]
public sealed class DiskIoSill : ISillActivatedByDefault, ISillSingleView, IDisposable
{
    private readonly DiskIoCollector _collector = new();
    private readonly System.Timers.Timer _timer;
    private bool _disposed;

    public event EventHandler? ContentChanged;

    // --- ISill ---
    public string DisplayName => "Disk I/O";
    public SillSettingsView[] SettingsViews => Array.Empty<SillSettingsView>();

    // --- ISillActivatedByDefault ---
    public ValueTask OnActivatedAsync() => ValueTask.CompletedTask;

    // --- ISill ---
    public ValueTask OnDeactivatedAsync() => ValueTask.CompletedTask;

    // --- ISill ---
    public IconElement CreateIcon() => new SymbolIcon(Symbol.Save);

    // --- ISillSingleView ---
    // SillView é um controle WinUI; Title/Subtitle não existem na API atual.
    // O conteúdo visual será expandido em versões futuras via controle customizado.
    public SillView View => new SillView();

    public DiskIoSill()
    {
        _timer = new System.Timers.Timer(1_000);
        _timer.Elapsed += (_, _) =>
        {
            _collector.Sample();
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
