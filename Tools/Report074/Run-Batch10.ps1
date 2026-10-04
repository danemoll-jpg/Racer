# 0.74: Part B again (the CR056 "Forest detail batch" copies of trees in the tunnel line cleared too), rides and mouth shots.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path; Set-Location $root
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Remove-Item Docs/Report074/author-notes.txt -ErrorAction SilentlyContinue
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='tunnel'; $env:AUTHOR_SCENES='StreetLoopGreybox,StreetLoopReverse,LakeWoods,ForestLoopReverse,MountainLoop,MountainLoopReverse'
& $r -Method Report074Author.Run -Out partB4 -Minutes 90
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partB-author-notes.txt -Force
Get-Content Docs/Report074/author-notes.txt | Select-String 'Free Roam terrain|added'
Remove-Item Docs/Report074/author-notes.txt
foreach($sc in 'MountainLoop','MountainLoopReverse','LakeWoods','ForestLoopReverse','StreetLoopReverse'){
 $env:PROBE_CASES="ride:file=Docs/Report074/storm-drain-tunnel.txt:moto:fwd:12;shots:tunnel-entrance-$sc|186,69,86|166,62,73|12|Clear"
 & $r -Method Report074Play.Run -Scenes $sc -Play 'Report074Checks' -Out 'verify9' -Minutes 30 | Select-Object -First 1
 git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
 Get-Content "$env:LOCALAPPDATA\Temp\report074\verify9\checks-$sc\results.txt" }
"BATCH10 DONE"
