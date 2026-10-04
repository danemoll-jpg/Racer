# 0.74 verification, second part in Street Loop Forward: Free Roam screenshots, the storm, the Free Roam clock, waypoints.
$r=Join-Path $PSScriptRoot 'Run-Unity.ps1'
$root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path; Set-Location $root
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
$shotsSLG='shots:BUG-001-after|452.1,84.05,11.33|434.74,81.95,4.91|12|Clear|BUG-002-after|507.17,84.62,-133.55|492.0,82.52,-122.97|12|Clear|BUG-003-gate-after|508,85,-133|490,77,-146|12|Clear|BUG-003-hillside-after|486,80,-150|470,70,-180|12|Clear|BUG-003-valley-after|420,60,-245|420,40,-200|12|Clear|tunnel-entrance|186,69,86|166,62,73|12|Clear|tunnel-gully|128,40,-60|140,32,-25|12|Clear|tunnel-entrance-snow|186,69,86|166,62,73|12|Snow|dawn-street|452.1,84.05,11.33|434.74,81.95,4.91|6.3|Clear|dusk-street|452.1,84.05,11.33|434.74,81.95,4.91|19.4|Clear'
$env:PROBE_CASES="$shotsSLG;storm:180:12;storm:180:0;clock;waypoint"
& $r -Method Report074Play.Run -Scenes 'StreetLoopGreybox' -Play 'Report074Checks' -Out 'verify2' -Minutes 45 | Select-Object -First 3
git checkout -- Assets/Resources/VehicleGlazing.mat 2>$null
Get-Content "$env:LOCALAPPDATA\Temp\report074\verify2\checks-StreetLoopGreybox\results.txt"
"BATCH6 DONE"
