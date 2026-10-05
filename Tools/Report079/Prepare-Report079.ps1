$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.79.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.79.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/78000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.79.0-review1
Scenery fixes: every house, garage and shop stands on a foundation down to the ground (steps, chimneys and downspouts too); no tree on a road, driveway or trail; all paved roads and driveways one asphalt grey (Scenery: New).
Green street-name signs at the junctions of S Cherokee Ln, Hwy 92 and Trickum Rd.
Free Roam screen: the day and clock on their own (top left); activity text only at an activity, during an attempt and after it; acorn count only when one is found; the minimap in Free Roam (J / B on or off, remembered).
Cameras on screen: V / X camera hint when driving starts and when the view changes; pause menu Camera view and TRAILER / PHOTO MODE (F8); in Trailer Mode a controls panel (click, or B then D-pad / A on a controller; H hides it with the HUD).
Rider gestures (Model: New): F / RB shakes a fist (in Trailer Mode RB keeps 0.5x, use F).
Bindings: Review/TRAILER_MODE.md.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every item and the measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report079/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report079/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report079-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.79.0-review1-Windows --previous-catalog Builds/LauncherRelease-78000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-78000/assets/game-manifest.json --out Builds/LauncherRelease-79000 --build 79000 --version 0.79.0-review1 --notes "0.78 scenery fixes (foundations under every building, no trees on roads or driveways, one asphalt grey), street-name signs, cameras on screen (hint, pause menu, Trailer Mode controls panel), Free Roam clock and HUD cleanup, Free Roam minimap. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
