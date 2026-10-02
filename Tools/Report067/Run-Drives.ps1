# 0.67 targeted neighbour drives (muted) for one evidence tag (before/after).
param([Parameter(Mandatory)][string]$Tag)
$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$sets=[ordered]@{
 'MountainLoopReverse'='Main:1540:1830:AI:moto:0;Main:1540:1830:AI:atv:0;Main:1540:1830:throttle:moto:0;Main:800:1300:throttle:moto:0;Main:800:1300:throttle:atv:0;Main:800:1300:AI:moto:0;Summit Traverse:0:0:AI:moto:0;Summit Traverse:0:0:AI:atv:0;Downhill Ridge Cut:0:0:AI:moto:0;Downhill Ridge Cut:0:0:AI:atv:0;Main:100:200:AI:moto:0'
 'MountainLoop'='Main:740:860:AI:moto:0;Main:740:860:AI:atv:0;Main:2560:2740:AI:moto:0;Summit Traverse:0:0:AI:moto:0;Summit Traverse:0:0:AI:atv:0;Climbing Ridge Cut:0:0:AI:moto:0'
 'DansBackyardForward'='Main:690:800:AI:moto:0;Main:690:800:AI:atv:0;Main:690:800:throttle:moto:0'
}
foreach($scene in $sets.Keys){
 $env:PROBE_DRIVE=$sets[$scene]
 & $run -Method Report067Play.Run -Scenes $scene -Play Report067DriveChecks -Out "drives-$Tag" -Minutes 45
}
