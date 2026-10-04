$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.74.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.74.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/73000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.74.0-review1
House 3 driveway: the steep straight drop is gone; the driveway winds down the hillside again (round the lake to the house), gentle enough for every vehicle both ways. The tree in the brick-house driveway moved onto the grass; the tall warning sign at the top of the House 3 driveway is gone. Mountain Forward: the holes at the trail edge after the start and the torn bank at the High Ridge stretch are smooth ground now.
Free Roam everywhere: the Backyard storm-drain tunnel (with its gully, rats and frozen flow in Snow) and, in the Mountain scenes, the same Echo Cave as Forest Forward. Map: a Storm drain tunnel landmark.
Rain: a softer, rounder rain sound; thunderstorms with presence - a strike every 8-25 s with a visible forked bolt, near cracks and far rumbles, distant rolls between strikes. Lightning flashes: Off keeps bolts and thunder without the screen flash.
Dawn: a new Time of Day (Dawn / Day / Dusk / Night) - low eastern sun, pink-gold light, blue shadows, ground mist; Free Roam passes through it in the early morning.
Free Roam keeps its clock (saved when you start a race, return to the menu or quit) on a 30-day calendar (Day N on the HUD); the moon shows the day's phase and lights the night a little more near full. Night races use a full moon.
Map: click anywhere (or move the cursor and press Waypoint) to set a destination; a tall beam marks it in the world and the HUD shows the distance and an arrow. Right-click / Clear removes it.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every item and the frame-rate measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report074/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report074/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report074-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.74.0-review1-Windows --previous-catalog Builds/LauncherRelease-73000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-73000/assets/game-manifest.json --out Builds/LauncherRelease-74000 --build 74000 --version 0.74.0-review1 --notes "House 3 driveway rebuilt gentle, driveway tree and sign, Mountain trail-edge holes; storm-drain tunnel and Forest Forward cave in every Free Roam; new rain sound and stronger thunderstorms with bolts; Dawn; saved Free Roam clock with a 30-day calendar and moon phases; map waypoints anywhere. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
