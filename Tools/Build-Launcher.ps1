param([switch]$CoreOnly)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $root 'Builds/Launcher'
New-Item -ItemType Directory -Force $out | Out-Null
$vc='C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat'
$script=Join-Path $root 'Temp/build-launcher.cmd'
$entry=if($CoreOnly){'core_cli.cpp'}else{'launcher.cpp'}
$subsystem=if($CoreOnly){'CONSOLE'}else{'WINDOWS'}
$exe=if($CoreOnly){'LauncherChecks.exe'}else{'WoodstockRushLauncher.exe'}
@"
@echo off
call "$vc" >nul
if errorlevel 1 exit /b %errorlevel%
set "INCLUDE=$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\ucrt;$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\shared;$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\um;%INCLUDE%"
set "LIB=$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp.x64\c\um\x64;$root\Builds\LauncherSDK\microsoft.windows.sdk.cpp.x64\c\ucrt\x64;%LIB%"
cl /nologo /TC /O2 /MT /c /Fo:"$out\miniz.obj" "$root\Launcher\vendor\miniz.c"
if errorlevel 1 exit /b %errorlevel%
cl /nologo /utf-8 /std:c++17 /EHsc /O2 /MT /W4 /Fe:"$out\$exe" /Fo:"$out\\" "$root\Launcher\$entry" "$root\Launcher\core.cpp" "$root\Launcher\store.cpp" "$out\miniz.obj" /link /SUBSYSTEM:$subsystem
"@ | Set-Content -LiteralPath $script
& cmd.exe /d /c $script
if($LASTEXITCODE -ne 0){throw "Launcher compilation failed: $LASTEXITCODE"}
Get-FileHash -LiteralPath "$out/$exe"
