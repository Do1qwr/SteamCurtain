param(
    [switch]$Uninstall,
    [switch]$NoAutostart,
    [switch]$NoStart
)

$ErrorActionPreference = 'Stop'

$appDir = Join-Path $env:LOCALAPPDATA 'SteamCurtain'
$targetExe = Join-Path $appDir 'SteamCurtain.exe'
$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$runName = 'SteamCurtain'
$procName = 'SteamCurtain'

function Stop-Curtain {
    Get-Process -Name $procName -ErrorAction SilentlyContinue | ForEach-Object {
        try { $_.Kill() } catch { }
    }
    Start-Sleep -Milliseconds 500
}

if ($Uninstall) {
    Stop-Curtain
    if (Get-ItemProperty -Path $runKey -Name $runName -ErrorAction SilentlyContinue) {
        Remove-ItemProperty -Path $runKey -Name $runName -Force
    }
    if (Test-Path $appDir) { Remove-Item -LiteralPath $appDir -Recurse -Force }
    Write-Output 'SteamCurtain removed.'
    return
}

$built = Join-Path $PSScriptRoot 'build\SteamCurtain.exe'
if (-not (Test-Path $built)) {
    Write-Output 'Building...'
    & (Join-Path $PSScriptRoot 'build.ps1')
}

if (-not (Test-Path $built)) { throw "Build output not found: $built" }

Stop-Curtain
New-Item -ItemType Directory -Path $appDir -Force | Out-Null
Copy-Item -LiteralPath $built -Destination $targetExe -Force

if (-not $NoAutostart) {
    Set-ItemProperty -Path $runKey -Name $runName -Value ('"' + $targetExe + '"')
}

if (-not $NoStart) {
    Start-Process -FilePath $targetExe
}

$ver = (Get-Item $targetExe).LastWriteTime
Write-Output ('Installed: ' + $targetExe)
Write-Output ('Version date: ' + $ver)
if (-not $NoAutostart) { Write-Output 'Autostart: enabled' }
