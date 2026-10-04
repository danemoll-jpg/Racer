# 0.74 targeted verification (rule 11): play-mode checks per scene (muted, isolated saves) and editor views after.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path; Set-Location $root
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
# the tunnel part of the storm-drain branch (from 15 m before the bore to 10 m after it)
python -c "
import math
P=[tuple(map(float,l.split())) for l in open('Docs/Report074/storm-drain-route.txt') if l.strip()]
m=open('Docs/Report074/storm-drain-route-meta.txt').read().split();a=float(m[1].split('..')[0]);b=float(m[1].split('..')[1])
s=0;out=[]
for i,p in enumerate(P):
    if i: s+=math.dist(P[i-1],p)
    if a-15<=s<=b+10: out.append(p)
open('Docs/Report074/storm-drain-tunnel.txt','w').write('\n'.join(f'{x:.3f} {y:.3f} {z:.3f}' for x,y,z in out))
print('tunnel points',len(out),'of',len(P))
"
$D='House 3 valley driveway'; $T='file=Docs/Report074/storm-drain-tunnel.txt'; $C='file=Docs/Report074/echo-cave-route.txt'
function Checks($scene,$cases,$out,$play='Report074Checks'){ $env:PROBE_CASES=$cases; & $r -Method Report074Play.Run -Scenes $scene -Play $play -Out $out -Minutes 60 | Select-Object -First 3; git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null }
$drive=(@('moto','atv','original','tourer') | ForEach-Object { "ride:${D}:${_}:fwd:10;ride:${D}:${_}:back:10" }) -join ';'
$shotsSLG='shots:BUG-001-after|452.1,84.05,11.33|434.74,81.95,4.91|12|Clear|BUG-002-after|507.17,84.62,-133.55|492.0,82.52,-122.97|12|Clear|BUG-003-gate-after|508,85,-133|490,77,-146|12|Clear|BUG-003-hillside-after|486,80,-150|470,70,-180|12|Clear|BUG-003-valley-after|420,60,-245|420,40,-200|12|Clear|tunnel-entrance|186,69,86|166,62,73|12|Clear|tunnel-gully|128,40,-60|140,32,-25|12|Clear|tunnel-entrance-snow|186,69,86|166,62,73|12|Snow|dawn-street|452.1,84.05,11.33|434.74,81.95,4.91|6.3|Clear|dusk-street|452.1,84.05,11.33|434.74,81.95,4.91|19.4|Clear'
Checks 'StreetLoopGreybox' "$drive;ride:${D}:moto:fwd:13;ride:${T}:moto:fwd:12;ride:${T}:moto:fwd:12:Snow;ride:${T}:atv:back:10;$shotsSLG;storm:180:12;storm:180:0;clock;waypoint" 'verify'
Checks 'ForestLoopReverse' $drive 'verify'
Checks 'LakeWoods' "ride:${D}:moto:fwd:10;ride:${D}:moto:back:10;ride:${D}:original:fwd:10;ride:${D}:original:back:10;ride:${T}:moto:fwd:12;shots:cave-forest-forward|-27.22,25.38,-36.16|-20.04,23.28,-53.2|12|Clear|cave-forest-forward-inside|59.7,37.4,87.0|87.4,40.4,157.5|12|Clear" 'verify'
Checks 'DansBackyardReverse' "ride:${D}:moto:fwd:10;ride:${D}:moto:back:10;ride:${D}:original:fwd:10;ride:${D}:original:back:10;ride:${T}:moto:fwd:12" 'verify'
Checks 'MountainLoop' "ride:pts=744,-110/743,-125/742.5,-135/742,-146:moto:fwd:20;ride:pts=744,-110/743,-125/742.5,-135/742,-146:atv:fwd:20;ride:pts=744,-110/743,-125/742.5,-135/742,-146:moto:fwd:26;ride:pts=1048,8/1047,25/1045,40/1043,52:moto:fwd:20;ride:pts=1048,8/1047,25/1045,40/1043,52:atv:fwd:20;ride:pts=1048,8/1047,25/1045,40/1043,52:moto:fwd:26;ride:pts=743,-128/747,-123/753,-121/760,-115/766,-108:moto:fwd:20;ride:pts=743,-128/747,-123/753,-121/760,-115/766,-108:atv:fwd:16;ride:${C}:moto:fwd:14;ride:${T}:moto:fwd:12;ride:${T}:moto:fwd:12:Snow;shots:cave-mountain-bug006|-27.22,25.38,-36.16|-20.04,23.28,-53.2|12|Clear|cave-mountain-inside|59.7,37.4,87.0|87.4,40.4,157.5|12|Clear|tunnel-entrance-mountain|186,69,86|166,62,73|12|Clear" 'verify'
Checks 'MountainLoopReverse' "ride:${C}:moto:fwd:14;ride:${T}:moto:fwd:12" 'verify'
Checks 'StreetLoopGreybox' 'race:Dawn:Clear:1' 'verify-race' 'Report073Checks'
$env:PROBE_NODEFAULT='1'; $env:PROBE_TAG='after'; $env:PROBE_EXTRA='BUG-001|StreetLoopGreybox|452.1,84.05,11.33|434.74,81.95,4.91;BUG-002|StreetLoopGreybox|507.17,84.62,-133.55|492.0,82.52,-122.97;BUG-003-top|StreetLoopGreybox|470,300,-180|470,0,-179.99|80;BUG-003-west|StreetLoopGreybox|360,90,-150|470,55,-160;BUG-004|MountainLoop|736.45,88.89,-119.79|744.48,86.79,-136.45;BUG-004-top|MountainLoop|742,200,-132|742,0,-131.99|14;BUG-005|MountainLoop|1043.71,153.57,18.59|1041.68,151.47,36.98;BUG-005-top|MountainLoop|1041,250,32|1041,0,32.01|14;BUG-006|MountainLoop|-27.22,25.38,-36.16|-20.04,23.28,-53.2;BUG-006-forward|LakeWoods|-27.22,25.38,-36.16|-20.04,23.28,-53.2'
& $r -Method Report074Views.Run -Out views-after -Minutes 20 | Select-Object -First 2
$env:PROBE_TAG='before'; Set-Location $root
"BATCH5 DONE"
