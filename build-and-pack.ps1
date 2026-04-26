param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = ".\WindowSill.DiskIo\WindowSill.DiskIo.csproj",

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = ".\artifacts"
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Host "Restoring WindowSill.DiskIo..." -ForegroundColor Cyan
dotnet restore $ProjectPath

Write-Host "Building WindowSill.DiskIo..." -ForegroundColor Cyan
dotnet build $ProjectPath -c $Configuration --no-restore

Write-Host "Packing WindowSill.DiskIo..." -ForegroundColor Cyan
dotnet pack $ProjectPath -c $Configuration --no-build -o $OutputDir

$nupkg = Get-ChildItem -Path $OutputDir -Filter "WindowSill.DiskIo*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $nupkg) {
    throw "Nenhum .nupkg WindowSill.DiskIo foi gerado."
}

$wsext = Join-Path $OutputDir ($nupkg.BaseName + ".wsext")
Copy-Item $nupkg.FullName $wsext -Force

Write-Host "OK .nupkg: $($nupkg.FullName)" -ForegroundColor Green
Write-Host "OK .wsext: $wsext" -ForegroundColor Green
Write-Host "`nProximo passo: .\install-diskio-extension.ps1 -ExtensionPath $wsext" -ForegroundColor Yellow
