# 0.74: Part B with line trees cleared in every scene, BUG-004 patch enlarged, then the clock/waypoint checks and views.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path; Set-Location $root
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Remove-Item Docs/Report074/author-notes.txt -ErrorAction SilentlyContinue
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='tunnel'; $env:AUTHOR_SCENES='StreetLoopGreybox,StreetLoopReverse,LakeWoods,ForestLoopReverse,MountainLoop,MountainLoopReverse'
& $r -Method Report074Author.Run -Out partB3 -Minutes 90
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partB-author-notes.txt -Force
Get-Content Docs/Report074/author-notes.txt | Select-String 'Free Roam terrain|added'
Remove-Item Docs/Report074/author-notes.txt
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='b4'; $env:AUTHOR_SCENES='MountainLoop'
& $r -Method Report074Author.Run -Out partA-b4 -Minutes 30
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partA-b4-author-notes.txt -Force
Get-Content Docs/Report074/author-notes.txt
Remove-Item Docs/Report074/author-notes.txt
$env:PROBE_CASES='clock;waypoint'
& $r -Method Report074Play.Run -Scenes 'StreetLoopGreybox' -Play 'Report074Checks' -Out 'verify4' -Minutes 30 | Select-Object -First 3
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Get-Content "$env:LOCALAPPDATA\Temp\report074\verify4\checks-StreetLoopGreybox\results.txt"
$env:PROBE_CASES='ride:pts=744,-110/743,-125/742.5,-135/742,-146:moto:fwd:20;ride:pts=744,-110/743,-125/742.5,-135/742,-146:atv:fwd:20;ride:pts=743,-128/747,-123/753,-121/760,-115/766,-108:moto:fwd:20;ride:file=Docs/Report074/storm-drain-tunnel.txt:moto:fwd:12;shots:tunnel-entrance-mountain|186,69,86|166,62,73|12|Clear'
& $r -Method Report074Play.Run -Scenes 'MountainLoop' -Play 'Report074Checks' -Out 'verify5' -Minutes 30 | Select-Object -First 3
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Get-Content "$env:LOCALAPPDATA\Temp\report074\verify5\checks-MountainLoop\results.txt"
$env:PROBE_NODEFAULT='1'; $env:PROBE_TAG='after'; $env:PROBE_EXTRA='BUG-004|MountainLoop|736.45,88.89,-119.79|744.48,86.79,-136.45;BUG-004-top|MountainLoop|738,200,-132|738,0,-131.99|16'
& $r -Method Report074Views.Run -Out views-after2 -Minutes 20 | Select-Object -First 2
"BATCH8 DONE"
