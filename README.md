# WindowSill Disk I/O Extension

Exibe métricas de leitura/escrita de disco em tempo real na barra do [WindowSill](https://getwindowsill.app).

## O que aparece na barra

```
R 12.3  W 4.1 MB/s  Q:0
```

- **R** = Read MB/s
- **W** = Write MB/s  
- **Q** = Current Disk Queue Length

Tooltip ao passar o mouse exibe os valores com 2 casas decimais.

## Pré-requisitos

- Windows 10/11 x64
- [WindowSill](https://getwindowsill.app) instalado
- [.NET 9 SDK](https://aka.ms/dotnet/download)
- Visual Studio 2022 com workload **WinUI application development** + **Windows 11 SDK (10.0.22621.0)**

## Como localizar a DLL do WindowSill.API

Após instalar o WindowSill, localize o arquivo `WindowSill.API.dll`:

```powershell
Get-ChildItem "$env:LOCALAPPDATA\Microsoft\WindowsApps" -Filter "WindowSill.API.dll" -Recurse -ErrorAction SilentlyContinue
Get-ChildItem "$env:PROGRAMFILES\WindowsApps" -Filter "WindowSill.API.dll" -Recurse -ErrorAction SilentlyContinue 2>$null
```

Atualize o `<HintPath>` no `.csproj` com o caminho encontrado.

## Build

```powershell
.\build-and-pack.ps1
```

## Instalação no WindowSill

```powershell
.\install-diskio-extension.ps1 -ExtensionPath .\artifacts\WindowSill.DiskIo.0.1.0.wsext
```

## Contadores usados

| Counter | Categoria | Descrição |
|---|---|---|
| Disk Read Bytes/sec | PhysicalDisk | Bytes lidos por segundo |
| Disk Write Bytes/sec | PhysicalDisk | Bytes escritos por segundo |
| Current Disk Queue Length | PhysicalDisk | Requisições aguardando na fila |
