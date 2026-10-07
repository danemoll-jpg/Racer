# 0.90 evidence: a scratch test player (frame-timing stats on) runs SplitBench at 3840x2160 (split-screen, player 2 AI).
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..')); Set-Location $root
$t="$env:LOCALAPPDATA\Temp\report090"; $o="$t\bench"; New-Item -ItemType Directory -Force $o | Out-Null
$env:REPORT090_OUT="$t\build-bench"; if(Test-Path $env:REPORT090_OUT){Remove-Item -Recurse -Force $env:REPORT090_OUT}
& Tools/Report090/Run-Unity.ps1 -Method Report090Build.Test -Out "build-bench" -Minutes 40 | Select-Object -First 1
Get-Content "$env:REPORT090_OUT\build-result.txt"
$a=@('-splitBench',"$o\run",'-racerTestSave',"$o\save",'-screen-fullscreen','1','-screen-width','3840','-screen-height','2160','-logFile',"$o\run.log")
$p=Start-Process "$env:REPORT090_OUT\Racer.exe" -ArgumentList $a -PassThru; if(!$p.WaitForExit(1800000)){Stop-Process -Id $p.Id -Force; "TIMEOUT"}
Get-Content "$o\run\split.txt"
"BENCH DONE"
