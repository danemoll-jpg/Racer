# 0.68 Part A targeted reset / fall-through checks (muted) for one evidence tag (before = 0.67 code, after = 0.68).
param([Parameter(Mandatory)][string]$Tag,[string]$Only='')
$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$sets=[ordered]@{
 'MountainLoop'='offlanding:moto:2190:690,87,75;drop:moto:760,177,82:-29,-5,-8;drop:moto:740,170,80:-30,-20,-9;drop:atv:760,177,82:-29,-5,-8;overshoot:moto:2080:30;overshoot:moto:2080:40;overshoot:moto:2080:46;overshoot:atv:2080:42;offside:moto:1500:22;upside:moto:1000;below:moto:800;quit:moto:728.22,79.2,-10.35;quit:moto:1013,160,96;roambelow:moto:800;roamspawn:moto'
 'MountainLoopReverse'='undershoot:moto:880:16;undershoot:moto:880:20;offside:moto:600:20'
 'DansBackyardForward'='offside:moto:737.9:12;upside:moto:600'
}
foreach($scene in $sets.Keys){
 if($Only -and $scene -ne $Only){continue}
 $env:PROBE_RESET=$sets[$scene]
 & $run -Method Report068Play.Run -Scenes $scene -Play Report068ResetChecks -Out "reset-$Tag" -Minutes 25
}
