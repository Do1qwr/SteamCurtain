param(
    [string]$OutDir = "$PSScriptRoot\build"
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot 'SteamCurtain.cs'
$ico = Join-Path $PSScriptRoot 'icon.ico'

if (-not (Test-Path $src)) { throw "Source not found: $src" }

$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path $csc)) { throw "C# compiler not found (need .NET Framework 4.x)" }

New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
$exe = Join-Path $OutDir 'SteamCurtain.exe'

$args = @('/nologo', '/target:winexe', '/optimize+', "/out:$exe")
if (Test-Path $ico) { $args += "/win32icon:$ico" }
$args += $src

& $csc $args
if ($LASTEXITCODE -ne 0) { throw "Build failed with code $LASTEXITCODE" }

Write-Output ("Built: " + $exe)
