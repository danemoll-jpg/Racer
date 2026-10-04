# 0.74: Part B again (trees cleared from the tunnel line in Free Roam), then the Street Loop Forward verification part 2.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path; Set-Location $root
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Remove-Item Docs/Report074/author-notes.txt -ErrorAction SilentlyContinue
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='tunnel'; $env:AUTHOR_SCENES='StreetLoopGreybox,StreetLoopReverse,LakeWoods,ForestLoopReverse,MountainLoop,MountainLoopReverse'
& $r -Method Report074Author.Run -Out partB2 -Minutes 90
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partB-author-notes.txt -Force
Get-Content Docs/Report074/author-notes.txt | Select-String 'Free Roam terrain|added'
Remove-Item Docs/Report074/author-notes.txt
& (Join-Path $PSScriptRoot 'Run-Batch6.ps1')
$env:PROBE_CASES='ride:file=Docs/Report074/storm-drain-tunnel.txt:moto:fwd:12;ride:file=Docs/Report074/storm-drain-tunnel.txt:atv:back:10:Snow;shots:tunnel-entrance-mountain|186,69,86|166,62,73|12|Clear|tunnel-gully-mountain|128,40,-60|140,32,-25|12|Clear|tunnel-inside-mountain|146,58,58|142,48,36|23|Clear'
& $r -Method Report074Play.Run -Scenes 'MountainLoop' -Play 'Report074Checks' -Out 'verify3' -Minutes 30 | Select-Object -First 3
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Get-Content "$env:LOCALAPPDATA\Temp\report074\verify3\checks-MountainLoop\results.txt"
"BATCH7 DONE"
