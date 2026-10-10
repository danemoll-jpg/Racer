# Runs a player build with an evidence component: Run-Player.ps1 -Out name -PlayerArgs "-p104kind views ..." [-Build Builds/Check104] [-Check p104check] [-Minutes 20]
param([Parameter(Mandatory)][string]$Out,[Parameter(Mandatory)][string]$PlayerArgs,[string]$Build='Builds/Check104',[string]$Check='p104check',[int]$Minutes=20,[switch]$EmptySave,[int]$Width=1920,[int]$Height=1080,[switch]$Visible)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$scratch=Join-Path $env:LOCALAPPDATA 'Temp/report104'; $outFull=Join-Path $scratch $Out; if(Test-Path $outFull){Remove-Item -Recurse -Force $outFull}; New-Item -ItemType Directory -Force $outFull | Out-Null
# an isolated copy of Dan's save: the player never touches the real one
$dan=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3'
$save=Join-Path $outFull 'save-copy'; New-Item -ItemType Directory -Force $save | Out-Null
if(!$EmptySave){Get-ChildItem $dan -File | Where-Object { $_.Extension -in '.json','.bak' } | ForEach-Object { Copy-Item $_.FullName $save }}
$argList="-$Check `"$outFull`" -racerTestSave `"$save`" -screen-fullscreen 0 -screen-width $Width -screen-height $Height -logFile `"$outFull/player.log`" $PlayerArgs"
$p=Start-Process -FilePath (Join-Path $root "$Build/Racer.exe") -ArgumentList $argList -PassThru -WindowStyle $(if($Visible){"Normal"}else{"Minimized"})
if(!$p.WaitForExit($Minutes*60000)){Stop-Process -Id $p.Id -Force;"TIMEOUT"}
"exit=$($p.ExitCode) out=$outFull"; Get-ChildItem $outFull -Filter '*-check.txt' | ForEach-Object { Get-Content $_.FullName }
