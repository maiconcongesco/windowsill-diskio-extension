using System.Diagnostics;

namespace WindowSill.DiskIo;

/// <summary>
/// Coleta metricas de I/O do disco via PerformanceCounters nativos do Windows.
/// </summary>
internal sealed class DiskIoCollector : IDisposable
{
    private readonly PerformanceCounter _readCounter;
    private readonly PerformanceCounter _writeCounter;
    private readonly PerformanceCounter _queueCounter;
    private bool _disposed;

    public DiskIoCollector(string instance = "_Total")
    {
        _readCounter  = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec",       instance, readOnly: true);
        _writeCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec",      instance, readOnly: true);
        _queueCounter = new PerformanceCounter("PhysicalDisk", "Current Disk Queue Length", instance, readOnly: true);

        // Primeira leitura descartada (Windows sempre retorna 0 na primeira chamada)
        _ = _readCounter.NextValue();
        _ = _writeCounter.NextValue();
        _ = _queueCounter.NextValue();
    }

    public DiskIoSnapshot Sample() => new(
        ReadBytesPerSec:  (long)_readCounter.NextValue(),
        WriteBytesPerSec: (long)_writeCounter.NextValue(),
        QueueLength:      (int)Math.Round(_queueCounter.NextValue())
    );

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _readCounter.Dispose();
        _writeCounter.Dispose();
        _queueCounter.Dispose();
    }
}

internal record DiskIoSnapshot(
    long ReadBytesPerSec,
    long WriteBytesPerSec,
    int  QueueLength)
{
    public double ReadMbps  => ReadBytesPerSec  / 1_048_576.0;
    public double WriteMbps => WriteBytesPerSec / 1_048_576.0;

    public string BarText =>
        $"R {ReadMbps:F1}  W {WriteMbps:F1} MB/s  Q:{QueueLength}";

    public string TooltipText =>
        $"Leitura: {ReadMbps:F2} MB/s\n" +
        $"Escrita: {WriteMbps:F2} MB/s\n" +
        $"Fila:    {QueueLength} req";
}
