# Runs the check build with the 0.102 evidence component: Run-Player.ps1 -Out name -Args "-p99kind views ..." [-Build Builds/Check103] [-Minutes 20]
param([Parameter(Mandatory)][string]$Out,[Parameter(Mandatory)][string]$PlayerArgs,[string]$Build='Builds/Check103',[int]$Minutes=20,[switch]$EmptySave)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$scratch=Join-Path $env:LOCALAPPDATA 'Temp/report103'; $outFull=Join-Path $scratch $Out; if(Test-Path $outFull){Remove-Item -Recurse -Force $outFull}; New-Item -ItemType Directory -Force $outFull | Out-Null
# an isolated copy of Dan's save: the player never touches the real one
$dan=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3'
$save=Join-Path $outFull 'save-copy'; New-Item -ItemType Directory -Force $save | Out-Null
if(!$EmptySave){Get-ChildItem $dan -File | Where-Object { $_.Extension -in '.json','.bak' } | ForEach-Object { Copy-Item $_.FullName $save }}
$argList="-p103check `"$outFull`" -racerTestSave `"$save`" -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -logFile `"$outFull/player.log`" $PlayerArgs"
$p=Start-Process -FilePath (Join-Path $root "$Build/Racer.exe") -ArgumentList $argList -PassThru
if(!$p.WaitForExit($Minutes*60000)){Stop-Process -Id $p.Id -Force;"TIMEOUT"}
"exit=$($p.ExitCode) out=$outFull"; Get-Content (Join-Path $outFull 'p103-check.txt') -ErrorAction SilentlyContinue
