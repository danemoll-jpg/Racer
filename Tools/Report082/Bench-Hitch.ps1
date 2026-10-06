# 0.82 Part C: builds a scratch test player (-Dev: development, so the profiler markers report) and runs HitchBench at
# 3840x2160: a Street Loop race start and a 75 s Free Roam drive along Hwy 92 / S Cherokee Ln with traffic.
param([string]$Tag='before',[switch]$Dev,[switch]$NoBuild,[switch]$Deep)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..')); Set-Location $root
$t="$env:LOCALAPPDATA\Temp\report082"; $kind=if($Dev){'dev'}else{'rel'}
$env:REPORT082_OUT="$t\build-$kind"; $env:REPORT082_DEV=if($Dev){'1'}else{'0'}
if(!$NoBuild){ if(Test-Path $env:REPORT082_OUT){Remove-Item -Recurse -Force $env:REPORT082_OUT}
 & Tools/Report082/Run-Unity.ps1 -Method Report082Build.Test -Out "build-$kind-log" -Minutes 40 | Select-Object -First 3
 Get-Content "$env:REPORT082_OUT\build-result.txt" }
$o="$t\hitch-$Tag-$kind"; if(Test-Path $o){Remove-Item -Recurse -Force $o}; New-Item -ItemType Directory -Force $o | Out-Null
$a=@('-hitchBench',$o,'-racerTestSave',"$o\save",'-screen-fullscreen','1','-screen-width','3840','-screen-height','2160','-logFile',"$o\player.log"); if($Dev){$a+='-hitchProfile'}; if($Deep){$a+='-deepprofiling'}
$p=Start-Process "$env:REPORT082_OUT\Racer.exe" -ArgumentList $a -PassThru; if(!$p.WaitForExit(900000)){Stop-Process -Id $p.Id -Force; "TIMEOUT"}
Get-Content "$o\hitches.txt" | Select-String '^(==|screen|free roam)' | ForEach-Object { $_.Line }
if($Dev){ foreach($r in Get-ChildItem "$o\*.raw"){ $env:PROFILE_RAW=$r.FullName; & Tools/Report082/Run-Unity.ps1 -Method Report082Profile.Run -Out "hitch-$Tag-$kind" -Minutes 20 -NoGraphics | Select-Object -First 1 } }
