$ErrorActionPreference = "Stop"
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $vs) { throw "MSVC x86 tools were not found." }
$vcvars = Join-Path $vs "VC\Auxiliary\Build\vcvarsall.bat"
$here = $PSScriptRoot
$dll = Join-Path $here "FarmBotAction.dll"
$load = Join-Path $here "FarmBotLoad.exe"
cmd /c "`"$vcvars`" x86 && cl /nologo /EHsc /O2 /LD /Fe`"$dll`" `"$here\FarmBotAction.cpp`" /link /DLL user32.lib kernel32.lib psapi.lib d3d9.lib d3d11.lib dxgi.lib d3dcompiler.lib && cl /nologo /EHsc /O2 /Fe`"$load`" `"$here\FarmBotLoad.cpp`" /link user32.lib kernel32.lib"
if ($LASTEXITCODE -ne 0) { throw "FarmBotAction build failed." }
Write-Host "built $dll"
Write-Host "built $load"
