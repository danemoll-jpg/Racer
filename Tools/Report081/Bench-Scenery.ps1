# 0.81 evidence: a scratch test player (frame-timing stats on) runs ConditionsBench at 3840x2160 with -conditionsScenery:
# GPU frame times with Scenery New and Classic at the 0.71 views plus a woods view and the mountain summit, with shots.

$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..')); Set-Location $root
$t="$env:LOCALAPPDATA\Temp\report081"; $o="$t\bench"; New-Item -ItemType Directory -Force $o | Out-Null
$env:REPORT081_OUT="$t\build-bench"; if(Test-Path $env:REPORT081_OUT){Remove-Item -Recurse -Force $env:REPORT081_OUT}
& Tools/Report081/Run-Unity.ps1 -Method Report081Build.Test -Out "build-bench" -Minutes 40 | Select-Object -First 1
Get-Content "$env:REPORT081_OUT\build-result.txt"
function Bench($dir,$extra){ $a=@('-conditionsBench',$dir,'-racerTestSave',"$dir\save",'-screen-fullscreen','1','-screen-width','3840','-screen-height','2160','-logFile',"$dir.log")+$extra; $p=Start-Process "$env:REPORT081_OUT\Racer.exe" -ArgumentList $a -PassThru; if(!$p.WaitForExit(2700000)){Stop-Process -Id $p.Id -Force; "TIMEOUT $dir"} }
Bench "$o\warmup" @('-conditionsFpsOnly')
Bench "$o\run" @('-conditionsScenery')
Get-Content "$o\run\conditions.txt"
"BENCH DONE"
