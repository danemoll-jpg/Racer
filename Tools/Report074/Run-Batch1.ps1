# 0.74 development batch: BUG-003 in the five remaining scenes, their driveway profiles, and the read-only Part B/C surveys.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
Set-Location (Join-Path $PSScriptRoot '../..'); git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
$env:AUTHOR_DRY='0'; $env:AUTHOR_PARTS='drive'; $env:AUTHOR_SCENES='LakeWoods,DansBackyardForward,DansBackyardReverse,MountainLoop,MountainLoopReverse'
& $r -Method Report074Author.Run -Out drive13 -Minutes 40
Get-Content Docs/Report074/author-notes.txt | Select-String 'SCENE|benched|max grade|not found|gravel|in the driving width|tucked|dependents|moved|re-grounded|not moved'
& $r -Method Report074Prof.Run -Scenes 'LakeWoods,DansBackyardForward,DansBackyardReverse,MountainLoop,MountainLoopReverse' -Out prof8 -Minutes 15
Get-Content "$env:LOCALAPPDATA\Temp\report074\prof8\profile.txt" | Select-String -NotMatch '  p s|Rocky|42\d%|43\d%|44\d%|37\d%'
$env:PROBE_ROOTS='DansBackyardReverse|Reverse optional forest shortcuts|Storm Drain atmosphere;DansBackyardForward|Reverse optional forest shortcuts;StreetLoopGreybox|Reverse optional forest shortcuts;LakeWoods|Echo Cave enclosed rock|Ground_Echo wooded hillside|Echo Cave partial rockfall;MountainLoop|Echo Cave enclosed rock;ForestLoopReverse|Echo Cave enclosed rock|Forest Reverse natural cave atmosphere'
& $r -Method Report074Roots.Run -Out roots1 -Minutes 15
$env:PROBE_FILTER='^Ground_-?\d+_-?\d+$'; $env:PROBE_GRID='DansBackyardReverse|60,-80,190,90|2|drain-dbr;DansBackyardForward|60,-80,190,90|2|drain-dbf;StreetLoopGreybox|60,-80,190,90|2|drain-slg;MountainLoop|60,-80,190,90|2|drain-ml;LakeWoods|-60,-110,170,280|3|cave-lw;MountainLoop|-60,-110,170,280|3|cave-ml;ForestLoopReverse|-60,-110,170,280|3|cave-flr'
& $r -Method Report074Grid.Run -Out grid4 -Minutes 25
"BATCH1 DONE"
