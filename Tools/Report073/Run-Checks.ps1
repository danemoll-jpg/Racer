# 0.73 Parts A-C targeted play-mode checks (muted, isolated save), one batch editor per scene.
param([string]$Out='checks2',[string[]]$Only=@())
$ErrorActionPreference='Stop'
$runner=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$dan='water|pool|402.64,82.77,1.08|397.35,80.67,-16.65|house3|400,48,-168|400,33.5,-196|creek|446,27,-350|460.6,20.6,-365.8|drain|-.4,1,-3.5|@Shallow drain ripple footprint,0,0|fern|-6,5,-9|@Fern creek bed,.47,0|fern2|7,5,6|@Fern creek bed,-.47,0'
$sled='scenes|bug001|518.30,77.37,-147.03|520.90,75.27,-165.35|bug002|402.64,82.77,1.08|397.35,80.67,-16.65'
$lake='water2|lake|572,90,-20|642,78,-20|j1creek|348,84,218|364.5,73.9,234.9'
$runs=[ordered]@{
 'DansBackyardReverse'="scenes;views:$dan;views:$sled"
 'LakeWoods'="views:$lake;cross:Forest race route:330:430:moto:Clear;cross:Forest race route:330:430:moto:Snow;cross:Forest race route:330:430:atv:Clear;cross:Forest race route:330:430:atv:Snow;storm:4;race:Day:Snow:1"
 'ForestLoopReverse'='cross:line=384,-232/384,-156:0:72:moto:Clear;cross:line=384,-232/384,-156:0:72:moto:Snow;cross:line=384,-232/384,-156:0:72:atv:Clear;cross:line=384,-232/384,-156:0:72:atv:Snow'
}
foreach($scene in $runs.Keys){ if($Only.Count -gt 0 -and $Only -notcontains $scene){continue}
 $env:PROBE_CASES=$runs[$scene]; & $runner -Method Report073Play.Run -Scenes $scene -Play 'Report073Checks' -Out $Out -Minutes 40 | Select-Object -First 3 }
"CHECKS DONE"
