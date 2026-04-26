using System.Composition;
using WindowSill.API;

namespace WindowSill.DiskIo;

/// <summary>
/// Ponto de entrada MEF da extensao.
/// </summary>
[Export(typeof(IExtension))]
public sealed class ExtensionEntry : IExtension
{
    public string Id          => "WindowSill.DiskIo";
    public string DisplayName => "Disk I/O";
    public string Description => "Exibe metricas de leitura/escrita de disco (MB/s) e comprimento da fila na barra do WindowSill.";
    public Version Version    => new(0, 1, 0);
}
