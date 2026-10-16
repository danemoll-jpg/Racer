# 0.120: the round's built-player checks, one after another (Builds/Check120, test saves, windowed and minimized). Each writes its folder under
# %LOCALAPPDATA%/Temp/report120/<name>; a line per check to checks-done.txt. -Bumps: the bumps file being placed (StuntBumps -p120bumps, checks only).
param([string[]]$Only=@(),[string]$Bumps='')
$ErrorActionPreference='Continue'
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
$done=Join-Path $env:LOCALAPPDATA 'Temp/report120/checks-done.txt'
$b=if($Bumps){" -p120bumps `"$Bumps`""}else{''}
$checks=[ordered]@{
 # Part C: the course probes (the line, its ground, its gates and jumps, the rivals' speed) for placing the bumps
 probe4='-p106kind probe120 -p119course 4'; probe5='-p106kind probe120 -p119course 5'; probe8='-p106kind probe120 -p119course 8'; probe9='-p106kind probe120 -p119course 9'
 # Parts A, B, D: the park (alone and split screen)
 park='-p106kind park120'; split='-p106kind split120'
 # Parts A, B, C: a Stunt Race with the bumps (rivals on them, no reset there), and a clip over one bump
 race4="-p106kind race120 -p119course 4 -p115diff 2$b"; race9="-p106kind race120 -p119course 9 -p115diff 1$b"
 clip="-p106kind clip120 -p119course 4 -p120bump 1$b"
 # Part E: the garage by controller
 garage="-p106kind garage120$b"
}
foreach($k in $checks.Keys){
 if($Only.Count -and $k -notin $Only){continue}
 for($try=1;$try -le 2;$try++){
  $w=if($k -eq 'clip'){1280}else{1920}; $h=if($k -eq 'clip'){720}else{1080}
  $r=& $run -Out $k -PlayerArgs $checks[$k] -Minutes 45 -Width $w -Height $h
  $txt=Join-Path $env:LOCALAPPDATA "Temp/report120/$k/p106-check.txt"
  if((Test-Path $txt) -and (Get-Content $txt).Count -gt 1){break}
  "$k try ${try}: no check output (start-up crash?), again" | Add-Content $done
 }
 "$k done $(Get-Date -Format s)" | Add-Content $done
}
"ALL DONE" | Add-Content $done
