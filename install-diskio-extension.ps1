param(
    [Parameter(Mandatory = $true)]
    [string]$ExtensionPath,

    [Parameter(Mandatory = $false)]
    [switch]$SkipPolicyCheck
)

$ErrorActionPreference = "Stop"
$LogFile = "$env:TEMP\DiskIoExtensionInstall-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$windowSillExe = "C:\Program Files\WindowSill\WindowSill.exe"
$regPath = "HKLM:\SOFTWARE\WindowSill"
$tempDir = Join-Path $env:TEMP ("DiskIoExt-" + [guid]::NewGuid().Guid)

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $line = "[{0}] [{1}] {2}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $Level, $Message
    Write-Host $line
    Add-Content -Path $LogFile -Value $line
}

function Get-PackageIdFromWsext {
    param([string]$Path)
    New-Item -ItemType Directory -Force -Path $tempDir | Out-Null
    Expand-Archive -Path $Path -DestinationPath $tempDir -Force
    $nuspec = Get-ChildItem -Path $tempDir -Recurse -Filter "*.nuspec" | Select-Object -First 1
    if (-not $nuspec) { throw "Nenhum .nuspec encontrado no .wsext" }
    [xml]$xml = Get-Content $nuspec.FullName
    return $xml.package.metadata.id
}

try {
    Write-Log "== Disk I/O Extension Installer =="

    if (-not (Test-Path $windowSillExe)) { throw "WindowSill nao encontrado em $windowSillExe" }
    if (-not (Test-Path $ExtensionPath)) { throw "Arquivo nao encontrado: $ExtensionPath" }
    if (-not $ExtensionPath.EndsWith('.wsext')) { throw "O arquivo deve ter extensao .wsext" }

    $packageId = Get-PackageIdFromWsext -Path $ExtensionPath
    Write-Log "Package ID detectado: $packageId"

    if (-not $SkipPolicyCheck) {
        try {
            $allowed = (Get-ItemProperty -Path $regPath -Name "AllowedExtensions" -ErrorAction Stop).AllowedExtensions
            if ($allowed) {
                if ([string]::IsNullOrWhiteSpace($allowed)) { throw "Politica bloqueia todas as extensoes." }
                if ($packageId -notin ($allowed -split ';')) { throw "Package ID '$packageId' nao permitido em AllowedExtensions." }
                Write-Log "Package ID autorizado pela politica"
            }
        } catch {
            Write-Log "AllowedExtensions ausente; prosseguindo. ($($_.Exception.Message))" "WARN"
        }
    }

    Write-Log "Registrando WindowSill"
    & $windowSillExe /register
    Start-Sleep -Seconds 2

    Write-Log "Instalando Disk I/O Extension"
    Start-Process -FilePath $ExtensionPath -Wait

    Write-Log "Instalacao concluida com sucesso!" "SUCCESS"
    Write-Log "Log salvo em: $LogFile"
} catch {
    Write-Log $_.Exception.Message "ERROR"
    exit 1
} finally {
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
