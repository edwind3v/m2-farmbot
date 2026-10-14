$ErrorActionPreference = "Stop"
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $vs) { throw "MSVC x86 tools were not found." }
$vcvars = Join-Path $vs "VC\Auxiliary\Build\vcvarsall.bat"
$src = Join-Path $PSScriptRoot "ActionHarness.cpp"
$out = Join-Path $PSScriptRoot "ActionHarness.exe"
cmd /c "`"$vcvars`" x86 && cl /nologo /EHsc /O2 /Fe`"$out`" `"$src`" /link /SUBSYSTEM:CONSOLE"
if ($LASTEXITCODE -ne 0) { throw "ActionHarness build failed." }
Write-Host "built $out"
