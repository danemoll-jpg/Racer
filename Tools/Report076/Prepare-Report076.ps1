$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.76.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.76.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/75000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.76.0-review1
Races: every course is back to its 0.73 state around the House 3 driveway (Forest Loop Reverse races as before).
Free Roam now has its own world, built from Dan's Backyard Loop Reverse (dump, gullies, storm drain, the winding House 3 driveway). It is the same world whichever course is selected; you start at that course's start. Race Setup / Start Race / Return to Menu go back to the course. Map > Track shows any number of course routes over the world (Forest and Mountain routes dashed: race-only roads).
Rain storms: thunder you can hear over the engine and the radio, bolts in front of you that stay lit long enough to see, by day and by night.
Garage: the vehicle in true proportions from a three-quarter view; turn it with the right stick, Q / E or a mouse drag. Rider page: the rider stays fully in view while the list scrolls.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every item and the measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report076/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report076/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report076-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.76.0-review1-Windows --previous-catalog Builds/LauncherRelease-75000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-75000/assets/game-manifest.json --out Builds/LauncherRelease-76000 --build 76000 --version 0.76.0-review1 --notes "Races restored to 0.73 around the House 3 driveway; Free Roam in one dedicated world (from Dan's Backyard Loop Reverse) with course routes as map overlays; storms you can hear and see; garage preview with true proportions and rotation, Rider page preview always in view. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
