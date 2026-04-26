using System.Composition;
using WindowSill.API;

namespace WindowSill.DiskIo;

/// <summary>
/// Ativa o sill de Disk I/O sempre (ISillActivatedByDefault).
/// </summary>
[Export(typeof(ISillActivatedByDefault))]
internal sealed class DiskIoActivator : ISillActivatedByDefault
{
    public ValueTask OnActivatedAsync() => ValueTask.CompletedTask;
}
