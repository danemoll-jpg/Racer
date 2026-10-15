# 0.119: the round's built-player checks, one after another (Builds/Check119, test saves, windowed and minimized). Each writes its folder under
# %LOCALAPPDATA%/Temp/report119/<name>; a line per check to checks-done.txt.
param([string[]]$Only=@())
$ErrorActionPreference='Continue'
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
$done=Join-Path $env:LOCALAPPDATA 'Temp/report119/checks-done.txt'
$checks=[ordered]@{
 # Part A: Dan's spots (driver's view), BUG-004 at night
 shotsF='-p106kind shots119 -p115course 8 -p106vehicle drifter -p119time 1 -p118spots "A1-BUG-001-F|570.63|-88.85|354.4|2747.7;A2-BUG-002-F|526.29|-25.40|296.4|2835.3"'
 shotsR='-p106kind shots119 -p115course 9 -p106vehicle drifter -p119time 2 -p118spots "A3-BUG-003-R|-589.61|-531.84|139.6|2037.3;A4-BUG-004-R-night|459.83|-515.92|135.3|3418.1"'
 roamNight='-p106kind roam119 -p106vehicle drifter -p119hour 23 -p119spots "A4-roam-night-dan-spot|459.83|-515.92|135.3;A4-roam-night-mountain|700|-60|120"'
 # Part A: one AI race of Leviathan Forward (normal vehicles) through BUG-001's stretch and CP 18
 aiF='-p106kind lev115 -p115course 8 -p115runs 1 -p115laps 1 -p115diff 1 -p113traffic 0 -p115rivals "fastback+drifter+moto" -p113watch "571,-86,20"'
 # Part B: a Stunt Race on each course
 stunt4='-p106kind stunt119 -p119course 4 -p119split 0 -p115laps 1 -p115diff 0'
 stunt5='-p106kind stunt119 -p119course 5 -p119split 1 -p115laps 1 -p115diff 1'
 stunt8='-p106kind stunt119 -p119course 8 -p119split 0 -p115laps 1 -p115diff 1'
 stunt9='-p106kind stunt119 -p119course 9 -p119split 1 -p115laps 1 -p115diff 2'
 # Part C: the Stunt chapter from a test save
 campaign='-p106kind campaign119'
 # the new menu rows with the controller only
 menus='-p106kind menus119'
}
foreach($k in $checks.Keys){
 if($Only.Count -and $k -notin $Only){continue}
 for($try=1;$try -le 2;$try++){
  $r=& $run -Out $k -PlayerArgs $checks[$k] -Minutes 45
  $txt=Join-Path $env:LOCALAPPDATA "Temp/report119/$k/p106-check.txt"
  if((Test-Path $txt) -and (Get-Content $txt).Count -gt 1){break}
  "$k try ${try}: no check output (start-up crash?), again" | Add-Content $done
 }
 "$k done $(Get-Date -Format s)" | Add-Content $done
}
"ALL DONE" | Add-Content $done
