# 0.69 targeted neighbour drives (same sets as 0.68 so results compare like for like) (rule 11 / 5A.6), muted, using the 0.67 drive harness (Assets/Scripts/Report067DriveChecks.cs).
# Covers every 0.68 geometry change and the protected features next to it; no broad matrix.
param([Parameter(Mandatory)][string]$Tag,[string]$Only='')
$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$sets=[ordered]@{
 # Homeward Summit Flight (BUG-008 landing, bend runout + berm), start marker area, Summit Traverse entry berm, protected neighbours.
 'MountainLoop'='Main:2100:2650:AI:moto:0;Main:2100:2650:AI:atv:0;Main:2120:2600:throttle:moto:0;Main:2120:2600:throttle:atv:0;Main:2560:2740:AI:moto:0;Main:740:860:AI:moto:0;Summit Traverse:0:0:AI:moto:0;Summit Traverse:0:0:AI:atv:0;Summit Traverse:0:0:throttle:moto:0;Climbing Ridge Cut:0:0:AI:moto:0'
 # South Face jump + bend berm, BUG-006 shoulder, BUG-003/004 junction + lower main route (0.66), BUG-005 crease, Downhill Ridge Cut jumps + berm.
 'MountainLoopReverse'='Main:900:1400:AI:moto:0;Main:900:1400:AI:atv:0;Main:900:1400:throttle:moto:0;Main:900:1400:throttle:atv:0;Main:400:520:AI:moto:0;Main:400:520:throttle:atv:0;Main:1460:1830:AI:moto:0;Main:1460:1830:AI:atv:0;Main:2900:3010:AI:moto:0;Main:2900:3010:throttle:moto:0;Downhill Ridge Cut:0:0:AI:moto:0;Downhill Ridge Cut:0:0:AI:atv:0;Downhill Ridge Cut:0:0:throttle:moto:0;Downhill Ridge Cut:0:0:throttle:atv:0;Summit Traverse:0:0:AI:moto:0;Main:100:200:AI:moto:0'
}
foreach($scene in $sets.Keys){
 if($Only -and $scene -ne $Only){continue}
 $env:PROBE_DRIVE=$sets[$scene]
 & $run -Method Report069Play.Run -Scenes $scene -Play Report067DriveChecks -Out "drives-$Tag" -Minutes 60
}
