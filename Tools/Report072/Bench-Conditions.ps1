# 0.72 Part C evidence: a scratch test player (frame-timing stats on) runs ConditionsBench at 3840x2160:
# screenshots of the 9 time-of-day x weather combinations per course family, the cave at night, the Free Roam cycle,
# and GPU frame times at the 0.71 views for Day/Clear, Night/Clear, Day/Rain and Night/Snow.
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..')); Set-Location $root
$t="$env:LOCALAPPDATA\Temp\report072"; $o="$t\bench"; New-Item -ItemType Directory -Force $o | Out-Null
$env:REPORT072_OUT="$t\build-bench"; if(Test-Path $env:REPORT072_OUT){Remove-Item -Recurse -Force $env:REPORT072_OUT}
& Tools/Report072/Run-Unity.ps1 -Method Report072Build.Test -Out "build-bench" -Minutes 40 | Select-Object -First 1
Get-Content "$env:REPORT072_OUT\build-result.txt"
function Bench($dir,$extra){ $a=@('-conditionsBench',$dir,'-racerTestSave',"$dir\save",'-screen-fullscreen','1','-screen-width','3840','-screen-height','2160','-logFile',"$dir.log")+$extra; $p=Start-Process "$env:REPORT072_OUT\Racer.exe" -ArgumentList $a -PassThru; if(!$p.WaitForExit(1500000)){Stop-Process -Id $p.Id -Force; "TIMEOUT $dir"} }
Bench "$o\warmup" @('-conditionsFpsOnly')
Bench "$o\run" @()
Get-Content "$o\run\conditions.txt"
"BENCH DONE"
