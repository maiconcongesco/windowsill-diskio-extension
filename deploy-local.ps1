#Requires -Version 5.1
<#
.SYNOPSIS
    Instala o plugin WindowSill.DiskIo diretamente na pasta de plugins do WindowSill,
    criando toda a estrutura de pacote NuGet necessaria para o app reconhecer.
.DESCRIPTION
    Nao requer Visual Studio nem dotnet build bem-sucedido.
    Usa a dll ja compilada em bin\Release.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Step($msg) { Write-Host "[STEP] $msg" -ForegroundColor Cyan }
function Write-OK($msg)   { Write-Host "[ OK ] $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "[FAIL] $msg" -ForegroundColor Red; exit 1 }

# ---------------------------------------------------------------------------
# 1. Localizar a dll compilada
# ---------------------------------------------------------------------------
Write-Step "Localizando dll compilada..."
$dll = Get-ChildItem .\WindowSill.DiskIo\bin -Recurse -Filter "WindowSill.DiskIo.dll" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $dll) { Write-Fail "Dll nao encontrada em .\WindowSill.DiskIo\bin - execute dotnet build primeiro." }
Write-OK "Dll: $($dll.FullName) ($($dll.LastWriteTime))"

# ---------------------------------------------------------------------------
# 2. Localizar o WindowSill
# ---------------------------------------------------------------------------
Write-Step "Localizando WindowSill..."
$pkgBase = "$env:LOCALAPPDATA\Packages"
$wsPackage = Get-ChildItem $pkgBase -Directory | Where-Object { $_.Name -like "*WindowSill*" } | Select-Object -First 1
if (-not $wsPackage) { Write-Fail "WindowSill nao encontrado em $pkgBase" }
$pluginsRoot = Join-Path $wsPackage.FullName "LocalState\Plugins"
Write-OK "WindowSill: $($wsPackage.Name)"

# ---------------------------------------------------------------------------
# 3. Definir caminhos
# ---------------------------------------------------------------------------
$base    = Join-Path $pluginsRoot "WindowSill.DiskIo"
$libDir  = Join-Path $base "lib\net10.0-windows10.0.22621"
$relsDir = Join-Path $base "_rels"
$assetsDir        = Join-Path $base "Assets"
$contentAssets    = Join-Path $base "content\Assets"
$contentFilesAssets = Join-Path $base "contentFiles\any\net10.0-windows10.0.22621\Assets"
$pkgMeta          = Join-Path $base "package\services\metadata\core-properties"

# ---------------------------------------------------------------------------
# 4. Criar estrutura de diretorios
# ---------------------------------------------------------------------------
Write-Step "Criando estrutura de diretorios..."
@($libDir, $relsDir, $assetsDir, $contentAssets, $contentFilesAssets, $pkgMeta) | ForEach-Object {
    New-Item -ItemType Directory -Force -Path $_ | Out-Null
}
Write-OK "Diretorios criados."

# ---------------------------------------------------------------------------
# 5. Copiar dll
# ---------------------------------------------------------------------------
Write-Step "Copiando dll..."
Copy-Item $dll.FullName $libDir -Force
Write-OK "Dll copiada."

# ---------------------------------------------------------------------------
# 6. Escrever arquivos (usando .NET diretamente para evitar problemas de encoding/parsing)
# ---------------------------------------------------------------------------
Write-Step "Escrevendo arquivos de metadados..."
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

function Write-File($path, $content) {
    [System.IO.File]::WriteAllText($path, $content, $utf8NoBom)
}

# runtimeconfig.json
Write-File "$libDir\WindowSill.DiskIo.runtimeconfig.json" '{"runtimeOptions":{"tfm":"net10.0","framework":{"name":"Microsoft.NETCore.App","version":"10.0.0"}}}'

# WindowSill.DiskIo.nuspec
Write-File "$base\WindowSill.DiskIo.nuspec" @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>WindowSill.DiskIo</id>
    <version>0.2.0</version>
    <title>Disk I/O</title>
    <authors>maiconcongesco</authors>
    <description>Monitor your disk read/write activity without leaving your flow.</description>
    <license type="file">LICENSE.md</license>
    <icon>Assets/package.png</icon>
    <tags>windowsill diskio monitoring disk</tags>
    <repository type="git" url="https://github.com/maiconcongesco/windowsill-diskio-extension" />
  </metadata>
</package>
"@

# [Content_Types].xml
Write-File "$base\[Content_Types].xml" @"
<?xml version="1.0" encoding="utf-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels"   ContentType="application/vnd.openxmlformats-package.relationships+xml" />
  <Default Extension="psmdcp" ContentType="application/vnd.openxmlformats-package.core-properties+xml" />
  <Default Extension="dll"    ContentType="application/octet-stream" />
  <Default Extension="json"   ContentType="application/octet-stream" />
  <Default Extension="png"    ContentType="image/png" />
  <Default Extension="svg"    ContentType="image/svg+xml" />
  <Default Extension="md"     ContentType="application/octet-stream" />
  <Default Extension="nuspec" ContentType="application/octet-stream" />
</Types>
"@

# _rels/.rels
Write-File "$relsDir\.rels" @"
<?xml version="1.0" encoding="utf-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Type="http://schemas.microsoft.com/packaging/2010/07/manifest" Target="/WindowSill.DiskIo.nuspec" Id="R1" />
</Relationships>
"@

# .psmdcp
Write-File "$pkgMeta\diskio.psmdcp" @"
<?xml version="1.0" encoding="utf-8"?>
<coreProperties xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns="http://schemas.openxmlformats.org/package/2006/metadata/core-properties">
  <dc:creator>maiconcongesco</dc:creator>
  <dc:identifier>WindowSill.DiskIo</dc:identifier>
  <version>0.2.0</version>
  <keywords>windowsill diskio monitoring disk</keywords>
</coreProperties>
"@

# SVG icon
$svg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><ellipse cx="12" cy="6" rx="9" ry="3"/><path d="M3 6v6c0 1.66 4.03 3 9 3s9-1.34 9-3V6"/><path d="M3 12v6c0 1.66 4.03 3 9 3s9-1.34 9-3v-6"/></svg>'
Write-File "$contentAssets\diskio.svg" $svg
Write-File "$contentFilesAssets\diskio.svg" $svg

# LICENSE.md e CHANGELOGS.md
Copy-Item ".\WindowSill.DiskIo\LICENSE.md"    "$base\" -Force
Copy-Item ".\WindowSill.DiskIo\CHANGELOGS.md" "$base\" -Force

Write-OK "Todos os arquivos escritos."

# ---------------------------------------------------------------------------
# 7. Verificar estrutura final
# ---------------------------------------------------------------------------
Write-Step "Estrutura final:"
Get-ChildItem $base -Recurse | ForEach-Object { $_.FullName.Replace($base, '') }

# ---------------------------------------------------------------------------
# 8. Reiniciar WindowSill
# ---------------------------------------------------------------------------
Write-Step "Reiniciando WindowSill..."
Get-Process WindowSill -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
$appId = ($wsPackage.Name -replace '^64360VelerSoftware\.WindowSill_[^!]+', '') 
Start-Process "shell:AppsFolder\$($wsPackage.Name -replace '_[0-9\.]+_x64_', '_' -replace 'j80j2txgjg9dj$', 'j80j2txgjg9dj!App')"

Write-OK "Pronto! Verifique o WindowSill."
