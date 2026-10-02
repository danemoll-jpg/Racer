# 0.68 second verification round: BUG-005 vertical curve, Part E berm checks, like-for-like 0.67 comparison drives.
$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$env:AUTHOR_PARTS='b005'
& $run -Method Report068Author.Run -Out author-b005 -Minutes 30
$env:AUTHOR_PARTS='all'
$env:PROBE_RESET='line:moto:Main:2470:2580;line:atv:Main:2470:2580;overrun:moto:Main:2470:32;overrun:atv:Main:2470:32;line:moto:Summit Traverse:2:80;line:atv:Summit Traverse:2:80;overrun:moto:Summit Traverse:4:26;overrun:atv:Summit Traverse:4:26;pausemenu:moto'
& $run -Method Report068Play.Run -Scenes MountainLoop -Play Report068ResetChecks -Out berms -Minutes 30
$env:PROBE_RESET='line:moto:Main:1265:1370;line:atv:Main:1265:1370;overrun:moto:Main:1270:32;overrun:atv:Main:1270:32;line:moto:Downhill Ridge Cut:125:205;line:atv:Downhill Ridge Cut:125:205;overrun:moto:Downhill Ridge Cut:135:24;overrun:atv:Downhill Ridge Cut:135:24;line:moto:Main:2900:3000;line:atv:Main:2900:3000'
& $run -Method Report068Play.Run -Scenes MountainLoopReverse -Play Report068ResetChecks -Out berms -Minutes 30
$env:PROBE_DRIVE='Main:800:1300:throttle:moto:0;Main:800:1300:throttle:atv:0;Main:800:1300:AI:moto:0;Main:2900:3010:throttle:moto:0;Main:2900:3010:AI:moto:0;Main:2900:3010:throttle:atv:0'
& $run -Method Report068Play.Run -Scenes MountainLoopReverse -Play Report067DriveChecks -Out drives-round2 -Minutes 40
