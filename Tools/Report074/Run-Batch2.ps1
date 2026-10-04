# 0.74: Part A authoring in every scene from a clean state (BUG-001/002/003/004/005), the Part C dry run, driveway profiles.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
Set-Location (Join-Path $PSScriptRoot '../..'); git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Remove-Item Docs/Report074/author-notes.txt -ErrorAction SilentlyContinue
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='tree,sign,drive,b4,b5'; $env:AUTHOR_SCENES='StreetLoopGreybox,StreetLoopReverse,LakeWoods,ForestLoopReverse,DansBackyardForward,DansBackyardReverse,MountainLoop,MountainLoopReverse'
& $r -Method Report074Author.Run -Out partA -Minutes 60
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partA-author-notes.txt
Get-Content Docs/Report074/author-notes.txt | Select-String 'SCENE|benched|max grade|not found|gravel|re-grounded|not moved|removed|moved by|surface built|steepest'
Remove-Item Docs/Report074/author-notes.txt
$env:AUTHOR_DRY='1'; $env:AUTHOR_PARTS='cave'; $env:AUTHOR_SCENES='MountainLoop,MountainLoopReverse,ForestLoopReverse'
& $r -Method Report074Author.Run -Out cavedry -Minutes 30
Get-Content Docs/Report074/author-notes.txt
Remove-Item Docs/Report074/author-notes.txt
& $r -Method Report074Prof.Run -Scenes 'StreetLoopGreybox,LakeWoods,ForestLoopReverse,DansBackyardForward,DansBackyardReverse,MountainLoop,MountainLoopReverse' -Out profA -Minutes 20
Get-Content "$env:LOCALAPPDATA\Temp\report074\profA\profile.txt" | Select-String -NotMatch '  p s|Rocky|42\d%|43\d%|44\d%|37\d%|45\d%'
"BATCH2 DONE"
