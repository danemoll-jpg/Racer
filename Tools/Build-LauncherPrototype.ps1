$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $root 'Builds/LauncherPrototype'
New-Item -ItemType Directory -Force $out | Out-Null
$vc='C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat'
$script=Join-Path $root 'Temp/build-launcher-prototype.cmd'
@"
@echo off
call "$vc" >nul
if errorlevel 1 exit /b %errorlevel%
set "INCLUDE=$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\ucrt;$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\shared;$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\um;%INCLUDE%"
set "LIB=$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp.x64\c\um\x64;$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp.x64\c\ucrt\x64;%LIB%"
cl /nologo /std:c++17 /EHsc /O2 /MT /W4 /Fe:"$out\WoodstockRushLauncher.exe" /Fo:"$out\prototype.obj" "$root\Launcher\prototype.cpp" /link /SUBSYSTEM:WINDOWS
"@ | Set-Content -LiteralPath $script
& cmd.exe /d /c $script
if($LASTEXITCODE -ne 0){throw "Launcher prototype compilation failed: $LASTEXITCODE"}
Get-FileHash -LiteralPath "$out/WoodstockRushLauncher.exe"
