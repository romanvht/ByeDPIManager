#Requires -Version 5.1
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = $PSScriptRoot
$dependenciesPath = Join-Path $projectRoot 'redist'
$releasePath = Join-Path $projectRoot 'release'
$exeName = 'ByeDPI Manager.exe'

foreach ($folder in @('libs', 'redist')) {
  $path = Join-Path $dependenciesPath $folder
  if (-not (Test-Path -LiteralPath $path -PathType Container)) {
    throw "Missing dependency folder: $path. Place release dependencies there before running this script."
  }
  if (-not (Get-ChildItem -LiteralPath $path -File -Recurse -Force | Select-Object -First 1)) {
    throw "Dependency folder is empty: $path. Place release dependencies there before running this script."
  }
}

Get-Command dotnet -ErrorAction Stop | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem

$binPath = [System.IO.Path]::GetFullPath((Join-Path $projectRoot 'bin'))
$workPath = Join-Path $binPath ('release-' + [Guid]::NewGuid().ToString('N'))
$buildPath = Join-Path $workPath 'build'
$packagePath = Join-Path $workPath 'package'
$archivePath = Join-Path $workPath 'All_in_One_w64.zip'

try {
  New-Item -ItemType Directory -Path $buildPath, $packagePath -Force | Out-Null

  Write-Host 'Building ByeDPI Manager (Release)...'
  & dotnet build (Join-Path $projectRoot 'bdmanager.csproj') --configuration Release --output $buildPath --no-incremental
  if ($LASTEXITCODE -ne 0) {
    throw "Release build failed (exit code $LASTEXITCODE)."
  }

  $builtExe = Join-Path $buildPath $exeName
  if (-not (Test-Path -LiteralPath $builtExe -PathType Leaf)) {
    throw "Build did not produce $exeName."
  }

  Copy-Item -LiteralPath $builtExe -Destination $packagePath
  Copy-Item -LiteralPath (Join-Path $buildPath 'proxytest') -Destination $packagePath -Recurse
  foreach ($folder in @('libs', 'redist')) {
    Copy-Item -LiteralPath (Join-Path $dependenciesPath $folder) -Destination $packagePath -Recurse
  }

  Write-Host 'Creating All_in_One_w64.zip...'
  [System.IO.Compression.ZipFile]::CreateFromDirectory(
    $packagePath,
    $archivePath,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $false
  )

  New-Item -ItemType Directory -Path $releasePath -Force | Out-Null
  Copy-Item -LiteralPath $builtExe -Destination (Join-Path $releasePath $exeName) -Force
  Copy-Item -LiteralPath $archivePath -Destination (Join-Path $releasePath 'All_in_One_w64.zip') -Force
  Write-Host "Release ready: $releasePath"
}
finally {
  $resolvedWorkPath = [System.IO.Path]::GetFullPath($workPath)
  if (-not $resolvedWorkPath.StartsWith($binPath + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clean a temporary folder outside bin: $resolvedWorkPath"
  }
  if (Test-Path -LiteralPath $resolvedWorkPath) {
    Remove-Item -LiteralPath $resolvedWorkPath -Recurse -Force
  }
}
