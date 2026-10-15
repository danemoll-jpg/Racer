# 0.118: the round's built-player checks, one after another (Builds/Check118, test saves, windowed and minimized). Each writes its folder under
# %LOCALAPPDATA%/Temp/report118/<name>; a line per check to checks-done.txt.
param([string[]]$Only=@())
$ErrorActionPreference='Continue'
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
$done=Join-Path $env:LOCALAPPDATA 'Temp/report118/checks-done.txt'
$checks=[ordered]@{
 shotsF='-p106kind shots118 -p115course 8 -p106vehicle drifter -p118spots "A-BUG-001-F|1185.99|192.54|169.8|1880;A2-hill-up-foot-F|507.3|-22.3|20|672;A2-hill-up-mid-F|511.8|-12.1|31|684;A2-hill-down-top-F|525.4|-9.6|313|2852;A2-hill-down-mid-F|513.0|-10.7|218|2866" -p118bank "668,702;2848,2882;40,610"'
 shotsR='-p106kind shots118 -p115course 9 -p106vehicle drifter -p118spots "A-BUG-002-R|-611.04|-492.14|155.4|1962;B-BUG-003-R|696.87|11.48|120.3|4335;B-BUG-004-R|700.44|12.79|323.6|6083;A2-hill-up-mid-R|511.8|-12.1|23|4076;A2-hill-down-top-R|524.0|-9.0|270|6338" -p118bank "4058,4095;6332,6368"'
 hillUpF='-p106kind seg118 -p115course 8 -p106vehicle drifter -p118from 600 -p118to 735 -p118every 0.1 -p118view 2 -p118tag clip-hill-up-F'
 hillDownF='-p106kind seg118 -p115course 8 -p106vehicle drifter -p118from 2800 -p118to 2945 -p118every 0.1 -p118view 2 -p118tag clip-hill-down-F'
 aiR='-p106kind lev115 -p115course 9 -p115runs 2 -p115laps 1 -p115diff 1 -p113traffic 1 -p115rivals "fastback+drifter+moto|tourer+atv+roadster" -p113watch "510,-15,18"'
 aiF='-p106kind lev115 -p115course 8 -p115runs 2 -p115laps 1 -p115diff 1 -p113traffic 1 -p115rivals "fastback+drifter+moto|tourer+atv+roadster" -p113watch "510,-15,18"'
 resetF='-p106kind reset117 -p115course 8 -p106vehicle drifter -p117resets 684,2864'
 resetR='-p106kind reset117 -p115course 9 -p106vehicle drifter -p117resets 4076,6350'
 wwOutR='-p106kind seg118 -p115course 9 -p106vehicle drifter -p118from 4250 -p118to 4420 -p118lane 3 -p118every 0 -p118tag ww-out-right'
 wwOutL='-p106kind seg118 -p115course 9 -p106vehicle drifter -p118from 4250 -p118to 4420 -p118lane -3 -p118every 0 -p118tag ww-out-left'
 wwBackR='-p106kind seg118 -p115course 9 -p106vehicle drifter -p118from 6040 -p118to 6200 -p118lane 3 -p118every 0 -p118tag ww-back-right'
 wwBackL='-p106kind seg118 -p115course 9 -p106vehicle drifter -p118from 6040 -p118to 6200 -p118lane -3 -p118every 0 -p118tag ww-back-left'
 wwReset='-p106kind seg118 -p115course 9 -p106vehicle drifter -p118from 4250 -p118to 4420 -p118resets 4320,4345 -p118every 0 -p118tag ww-out-resets'
 wwResetB='-p106kind seg118 -p115course 9 -p106vehicle drifter -p118from 6040 -p118to 6200 -p118resets 6075,6095 -p118every 0 -p118tag ww-back-resets'
 segR='-p106kind seg118 -p115course 9 -p106vehicle drifter -p118from 4000 -p118to 6425 -p118every 0.2 -p118view 2 -p118pilot 1 -p118tag rec-R-cp14-cp21'
 rider='-p106kind rider118 -p118vehicles fastback,moto,stuntbike'
}
foreach($k in $checks.Keys){
 if($Only.Count -and $k -notin $Only){continue}
 for($try=1;$try -le 2;$try++){
  $r=& $run -Out $k -PlayerArgs $checks[$k] -Minutes 45
  $txt=Join-Path $env:LOCALAPPDATA "Temp/report118/$k/p106-check.txt"
  if((Test-Path $txt) -and (Get-Content $txt).Count -gt 1){break}
  "$k try ${try}: no check output (start-up crash?), again" | Add-Content $done
 }
 "$k done $(Get-Date -Format s)" | Add-Content $done
}
"ALL DONE" | Add-Content $done
