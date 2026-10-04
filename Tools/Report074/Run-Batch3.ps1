# 0.74: BUG-003 again in Backyard Reverse (level start at the gate), Part C (cave) in the Mountain scenes, Part B dry run.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
Set-Location (Join-Path $PSScriptRoot '../..'); git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Remove-Item Docs/Report074/author-notes.txt -ErrorAction SilentlyContinue
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='tree,sign,drive'; $env:AUTHOR_SCENES='DansBackyardReverse'
& $r -Method Report074Author.Run -Out partA-dbr -Minutes 30
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='cave'; $env:AUTHOR_SCENES='MountainLoop,MountainLoopReverse'
& $r -Method Report074Author.Run -Out partC -Minutes 30
Copy-Item Docs/Report074/author-notes.txt Docs/Report074/partA-dbr-partC-author-notes.txt
Get-Content Docs/Report074/author-notes.txt | Select-String 'SCENE|benched|max grade|gravel|Rocky|Part C'
Remove-Item Docs/Report074/author-notes.txt
$env:AUTHOR_DRY='1'; $env:AUTHOR_PARTS='tunnel,landmark'; $env:AUTHOR_SCENES='StreetLoopGreybox,LakeWoods,MountainLoop'
& $r -Method Report074Author.Run -Out tunneldry -Minutes 40
Get-Content Docs/Report074/author-notes.txt
Remove-Item Docs/Report074/author-notes.txt
& $r -Method Report074Prof.Run -Scenes 'DansBackyardReverse' -Out profB -Minutes 15
Get-Content "$env:LOCALAPPDATA\Temp\report074\profB\profile.txt" | Select-String -NotMatch '  p s|Rocky|42\d%|43\d%|44\d%|37\d%|45\d%'
"BATCH3 DONE"
