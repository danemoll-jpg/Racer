# 0.75 evidence: a scratch test player (frame-timing stats on) runs ConditionsBench at 3840x2160: GPU frame times at the
# 0.71 views (as 0.74) and the 0.75 mixed grid (New vs Classic models, Day and Night) with full-screen shots.

$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..')); Set-Location $root
$t="$env:LOCALAPPDATA\Temp\report075"; $o="$t\bench"; New-Item -ItemType Directory -Force $o | Out-Null
$env:REPORT075_OUT="$t\build-bench"; if(Test-Path $env:REPORT075_OUT){Remove-Item -Recurse -Force $env:REPORT075_OUT}
& Tools/Report075/Run-Unity.ps1 -Method Report075Build.Test -Out "build-bench" -Minutes 40 | Select-Object -First 1
Get-Content "$env:REPORT075_OUT\build-result.txt"
function Bench($dir,$extra){ $a=@('-conditionsBench',$dir,'-racerTestSave',"$dir\save",'-screen-fullscreen','1','-screen-width','3840','-screen-height','2160','-logFile',"$dir.log")+$extra; $p=Start-Process "$env:REPORT075_OUT\Racer.exe" -ArgumentList $a -PassThru; if(!$p.WaitForExit(1500000)){Stop-Process -Id $p.Id -Force; "TIMEOUT $dir"} }
Bench "$o\warmup" @('-conditionsFpsOnly')
Bench "$o\run" @('-conditionsFpsOnly','-conditionsModels')
Get-Content "$o\run\conditions.txt"
"BENCH DONE"
