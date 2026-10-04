# 0.74: BUG-004/005 rebuilt from the original sheets (BUG-004 keeps the bank's shape), clock check again, views, profiles.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path; Set-Location $root
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Remove-Item Docs/Report074/author-notes.txt -ErrorAction SilentlyContinue
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='b4fix,b4,b5'; $env:AUTHOR_SCENES='MountainLoop'
& $r -Method Report074Author.Run -Out partA-b45 -Minutes 30
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partA-b4-author-notes.txt -Force
Get-Content Docs/Report074/author-notes.txt
Remove-Item Docs/Report074/author-notes.txt
$env:PROBE_CASES='clock'
& $r -Method Report074Play.Run -Scenes 'StreetLoopGreybox' -Play 'Report074Checks' -Out 'verify6' -Minutes 30 | Select-Object -First 3
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Get-Content "$env:LOCALAPPDATA\Temp\report074\verify6\checks-StreetLoopGreybox\results.txt"
$env:PROBE_CASES='ride:pts=744,-110/743,-125/742.5,-135/742,-146:moto:fwd:20;ride:pts=744,-110/743,-125/742.5,-135/742,-146:atv:fwd:20;ride:pts=744,-110/743,-125/742.5,-135/742,-146:moto:fwd:26;ride:pts=1048,8/1047,25/1045,40/1043,52:moto:fwd:26;ride:pts=743,-128/747,-123/753,-121/760,-115/766,-108:moto:fwd:20;ride:pts=743,-128/747,-123/753,-121/760,-115/766,-108:atv:fwd:16'
& $r -Method Report074Play.Run -Scenes 'MountainLoop' -Play 'Report074Checks' -Out 'verify7' -Minutes 30 | Select-Object -First 3
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Get-Content "$env:LOCALAPPDATA\Temp\report074\verify7\checks-MountainLoop\results.txt"
$env:PROBE_NODEFAULT='1'; $env:PROBE_TAG='after'; $env:PROBE_EXTRA='BUG-004|MountainLoop|736.45,88.89,-119.79|744.48,86.79,-136.45;BUG-004-top|MountainLoop|738,200,-132|738,0,-131.99|16;BUG-005|MountainLoop|1043.71,153.57,18.59|1041.68,151.47,36.98'
& $r -Method Report074Views.Run -Out views-after3 -Minutes 20 | Select-Object -First 2
"BATCH9 DONE"
