$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.78.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.78.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/77000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.78.0-review1
New world scenery (Settings > Display > Scenery: New / Classic, default New): new trees and bushes, faceted rocks, redesigned houses and shops (Dan's house kept as it is, Kyle's house detailed only), lit windows at night, richer ground and roads, lake and creek banks with reeds, grass tufts near the camera. Visual only: every collider, road and route is unchanged.
Rider gestures (Model: New): F / RB shakes a fist (in Trailer Mode RB keeps 0.5x, use F); AI riders shake a fist at whoever runs into them; the race winner raises both fists.
Trailer Mode for filming and screenshots: F8, or pause menu > Trailer Mode. Nothing on screen (H: HUD, G: arrows / gates / beacon), cameras 1-9 (Chase, Orbit, Side, Front, Fixed, Flyover, Free, Auto, First person; D-pad on a controller), Z / LB slow motion 0.25x, X / RB 0.5x, P / F12 / R3 full-resolution screenshot without UI (Screenshots beside DebugReports). In Free Roam: time of day, clock, weather, moon phase and lightning on demand, never saved. A race run in Trailer Mode does not count for records.
Camera views in normal play: V / X cycles Chase, Far chase, First person and Front (remembered; records count in every view).
Bindings: Review/TRAILER_MODE.md.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every item and the measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report078/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report078/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report078-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.78.0-review1-Windows --previous-catalog Builds/LauncherRelease-77000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-77000/assets/game-manifest.json --out Builds/LauncherRelease-78000 --build 78000 --version 0.78.0-review1 --notes "World scenery upgrade (new trees, rocks, buildings, ground, roads and shores; Settings > Display > Scenery: New / Classic) and rider gestures (fist wave on F / RB, AI fist shakes, victory celebration). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
