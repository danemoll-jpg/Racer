# 1.0 check in the built runtime (the files that are published): debug hidden at launch, F3 / F4 / F6, the session kept across a relaunch,
# a quick race. Runs on a copy of Dan's save (its DebugReports stay in the copy), windowed and minimized.
param([string]$Exe=(Join-Path $PSScriptRoot '../../Builds/Racer-1.0.0-Windows/Racer.exe'))
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$work=Join-Path $env:LOCALAPPDATA 'Temp/report1000/debug-check'; if(Test-Path $work){Remove-Item -Recurse -Force $work}
$save=Join-Path $work 'save'; New-Item -ItemType Directory -Force $save | Out-Null
$dan=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3'
Get-ChildItem $dan -File | Where-Object { $_.Extension -in '.json','.bak' } | ForEach-Object { Copy-Item $_.FullName $save }
foreach($phase in 1,2){
 $out=Join-Path $work "phase$phase"; New-Item -ItemType Directory -Force $out | Out-Null
 $p=Start-Process -FilePath $Exe -ArgumentList @('-p106check',$out,'-p106kind','debug1000','-p1000phase',"$phase",'-racerTestSave',$save,'-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-p106w','1280','-p106h','720','-logFile',(Join-Path $out 'player.log')) -WindowStyle Minimized -PassThru
 if(!$p.WaitForExit(900000)){Stop-Process -Id $p.Id -Force;"phase $phase TIMEOUT"}
 "== phase $phase exit $($p.ExitCode)"; Get-Content (Join-Path $out 'p106-check.txt') -ErrorAction SilentlyContinue
}
