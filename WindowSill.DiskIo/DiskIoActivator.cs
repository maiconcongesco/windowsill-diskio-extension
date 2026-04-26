using System.ComponentModel.Composition;
using WindowSill.Sdk.Activation;

namespace WindowSill.DiskIo;

/// <summary>
/// Ativa o sill de Disk I/O sempre (ISillActivatedByDefault).
/// O sill fica permanentemente visivel na barra.
/// </summary>
[Export(typeof(ISillActivatedByDefault))]
[ActivationType(DiskIoActivator.InternalName)]
internal sealed class DiskIoActivator : ISillActivatedByDefault
{
    internal const string InternalName = "DiskIoAlwaysActivator";
}
