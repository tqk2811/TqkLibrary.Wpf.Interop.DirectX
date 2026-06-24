# CI build script: native (x64 + Win32) + managed pack. NO nuget push -- the release.yml
# workflow handles publishing (GitHub Release asset + optional nuget.org). Resolves the src/
# dir from this script's location, so it can be invoked from the repo root.
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..\src')).Path
Set-Location $root

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) { throw "vswhere not found at $vswhere" }
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($msbuild)) { throw "MSBuild not found via vswhere (install C++ workload)" }
Write-Host "MSBuild: $msbuild"

if (-not (Get-Command dotnet-gitversion -ErrorAction SilentlyContinue)) {
    dotnet tool install -g GitVersion.Tool | Out-Host
    $env:PATH = "$env:PATH;$env:USERPROFILE\.dotnet\tools"
}
$gv = dotnet-gitversion /output json | ConvertFrom-Json
$verMajor = [int]$gv.Major; $verMinor = [int]$gv.Minor; $verBuild = [int]$gv.CommitsSinceVersionSource
Write-Host "Version: $verMajor.$verMinor.$verBuild"

$header = @'
#pragma once
#define VER_FILE_MAJOR    {0}
#define VER_FILE_MINOR    {1}
#define VER_FILE_BUILD    {2}
#define VER_FILE_REVISION 0
#define VER_FILEVERSION_STR    "{0}.{1}.{2}.0"
#define VER_PRODUCTVERSION_STR "{0}.{1}.{2}"
'@ -f $verMajor, $verMinor, $verBuild
[System.IO.File]::WriteAllText("$root\TqkLibrary.Wpf.Interop.DirectX.Native\version.generated.h", $header + "`r`n", (New-Object System.Text.UTF8Encoding($false)))

Remove-Item -Recurse -Force .\x64\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\x86\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\TqkLibrary.Wpf.Interop.DirectX\bin\Release\** -ErrorAction SilentlyContinue

# Restore native NuGet packages (packages.config) before building
$sln = Get-ChildItem $root -Filter *.sln | Select-Object -First 1
if ($sln) { Write-Host "Restoring $($sln.Name) ..."; nuget restore $sln.FullName | Out-Host }

$nativeProj = "$root\TqkLibrary.Wpf.Interop.DirectX.Native\TqkLibrary.Wpf.Interop.DirectX.Native.vcxproj"
foreach ($platform in @('x64','Win32')) {
    Write-Host "Building native $platform ..."
    & $msbuild $nativeProj /t:Rebuild /p:Configuration=Release /p:Platform=$platform /p:SolutionDir="$root\" /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw "Native build failed ($platform)" }
}

dotnet pack .\TqkLibrary.Wpf.Interop.DirectX\TqkLibrary.Wpf.Interop.DirectX.csproj -c Release -o .\TqkLibrary.Wpf.Interop.DirectX\bin\Release
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }
$nupkg = Get-ChildItem .\TqkLibrary.Wpf.Interop.DirectX\bin\Release\*.nupkg | Select-Object -First 1
Write-Host "Packed: $($nupkg.Name)"
