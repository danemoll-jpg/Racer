# Builds the 0.101 check player, then runs the listed built-player checks one after another (a run that shows nothing in 4 minutes is restarted once).
param([string[]]$Runs=@('flights|-p101kind flights'),[switch]$NoBuild)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$stamp=if($NoBuild){(Get-ChildItem (Join-Path $root 'Builds') -Directory -Filter 'Check101-*' | Where-Object { $_.Name -match '^Check101-[0-9]{6}$' } | Sort-Object Name | Select-Object -Last 1).Name}else{'Check101-'+(Get-Date -Format 'HHmmss')}
if(!$NoBuild){$env:BUILD_OUT=Join-Path $root "Builds/$stamp";& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report101Build.Main -Out build-check -Minutes 40 | Select-Object -First 1;Get-Content (Join-Path $root "Builds/$stamp-build.txt")}
foreach($r in $Runs){$name,$args2=$r.Split('|',2)
 for($try=0;$try -lt 2;$try++){
  $out=Join-Path $env:LOCALAPPDATA ("Temp/report101/runs/$name-"+(Get-Date -Format "HHmmss"));New-Item -ItemType Directory -Force $out | Out-Null
  $dan=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3';$save=Join-Path $out 'save-copy';New-Item -ItemType Directory -Force $save | Out-Null
  Get-ChildItem $dan -File | Where-Object { $_.Extension -in '.json','.bak' } | ForEach-Object { Copy-Item $_.FullName $save }
  $p=Start-Process -FilePath (Join-Path $root "Builds/$stamp/Racer.exe") -ArgumentList "-p101check `"$out`" -racerTestSave `"$save`" -screen-fullscreen 0 -logFile `"$out/player.log`" -p101w 1280 -p101h 720 $args2" -PassThru
  $t0=Get-Date;$ok=$true
  while(!$p.HasExited){Start-Sleep 5;if(((Get-Date)-$t0).TotalMinutes -gt 4 -and !(Test-Path (Join-Path $out 'p101-check.txt'))){Stop-Process -Id $p.Id -Force;$ok=$false;break};if(((Get-Date)-$t0).TotalMinutes -gt 40){Stop-Process -Id $p.Id -Force;break}}
  if($ok){break}}
 "== $name $out";Get-Content (Join-Path $out 'p101-check.txt') -ErrorAction SilentlyContinue}
