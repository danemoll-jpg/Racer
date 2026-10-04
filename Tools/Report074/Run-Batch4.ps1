# 0.74: Part B in the six other scenes (Free Roam tunnel), the landmark in all eight, route exports for the rides.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
Set-Location (Join-Path $PSScriptRoot '../..'); git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Remove-Item Docs/Report074/author-notes.txt -ErrorAction SilentlyContinue
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='tunnel,landmark'; $env:AUTHOR_SCENES='StreetLoopGreybox,StreetLoopReverse,LakeWoods,ForestLoopReverse,MountainLoop,MountainLoopReverse,DansBackyardForward,DansBackyardReverse'
& $r -Method Report074Author.Run -Out partB -Minutes 90
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partB-author-notes.txt
Get-Content Docs/Report074/author-notes.txt
Remove-Item Docs/Report074/author-notes.txt
& $r -Method Report074Export.Run -Out export -Minutes 15
Get-Content Docs/Report074/storm-drain-route-meta.txt
"BATCH4 DONE"
