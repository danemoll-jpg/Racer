# 0.69 targeted checks (rule 11), muted, one scene per batch editor: Assets/Scripts/Report069Checks.cs.
param([Parameter(Mandatory)][string]$Tag,[string]$Only='')
$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$off='offon:Main:{0}:{1}:{2}:12'
$sets=[ordered]@{
 # BUG-008 barrier, the 0.68 Homeward berm, (Climbing Ridge Cut rejoin) and four worst Part A edge samples.
 'MountainLoop'='overrun:Climbing Ridge Cut:268:28:moto;overrun:Climbing Ridge Cut:268:28:atv;overrun:Climbing Ridge Cut:276:24:moto;line:Main:1360:1460:moto;line:Main:1360:1460:atv;line:Climbing Ridge Cut:240:296:moto;offon:Main:747:-1:moto:12;offon:Main:747:-1:atv:12;offon:Main:1825.5:-1:moto:12;offon:Main:1825.5:-1:atv:12;offon:Main:2665:1:moto:12;offon:Main:2665:1:atv:12;offon:Summit Traverse:170.5:-1:moto:12;offon:Summit Traverse:170.5:-1:atv:12;reset:1825.5:-1:moto;reset:747:-1:atv;line:Main:2470:2580:moto;line:Main:2470:2580:atv;overrun:Main:2470:32:moto;overrun:Main:2470:32:atv'
 # BUG-005 barrier, the 0.68 South Face and Downhill Ridge Cut berms, (crest bend + Summit Traverse entrance), BUG-004 and four worst Part A edge samples.
 'MountainLoopReverse'='overrun:Main:436:32:moto;overrun:Main:436:32:atv;overrun:Main:442:30:moto;overrun:Main:442:30:atv;line:Main:400:520:moto;line:Main:400:520:atv;line:Summit Traverse:0:60:moto;offon:Main:254:-1:moto:12;offon:Main:254:-1:atv:12;offon:Main:273:-1:moto:12;offon:Main:273:-1:atv:12;offon:Main:1665:1:moto:12;offon:Main:1665:1:atv:12;offon:Summit Traverse:300.5:1:moto:12;offon:Summit Traverse:300.5:1:atv:12;offon:Summit Traverse:452.5:1:moto:12;offon:Summit Traverse:452.5:1:atv:12;reset:254:-1:moto;reset:1665:1:atv;line:Main:1265:1370:moto;line:Main:1265:1370:atv;overrun:Main:1270:32:moto;overrun:Main:1270:32:atv;line:Downhill Ridge Cut:125:205:moto;line:Downhill Ridge Cut:125:205:atv;overrun:Downhill Ridge Cut:135:24:moto;overrun:Downhill Ridge Cut:135:24:atv'
}
foreach($scene in $sets.Keys){
 if($Only -and $scene -ne $Only){continue}
 $env:PROBE_CASES=$sets[$scene]
 & $run -Method Report069Play.Run -Scenes $scene -Play Report069Checks -Out "checks-$Tag" -Minutes 45
}
