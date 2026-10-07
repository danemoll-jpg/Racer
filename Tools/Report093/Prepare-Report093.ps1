$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.93.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.93.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/92000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.93.0-review1
Race starts: on Dan's Backyard with five or six on the grid every vehicle was jammed at GO until it reset; the grid rows are now 6 m apart.
Abandoned Cabin Jump (Dan's Backyard Forward): the landing and run-out to the rejoin are clear of brush, and a reset there lands on open trail.
The world map shows where the acorn areas are (tinted regions with names and counts), names the roads, houses, lake and summit, and has a north arrow; D-pad up / down picks an area.
The Exploration page lists each acorn area with where it is; choosing one opens the map on it. The Free Roam minimap names the area you are in.
The minimap has a compass (N on the rim, ticks for E, S and W).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report093-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.93.0-review1-Windows --previous-catalog Builds/LauncherRelease-92000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-92000/assets/game-manifest.json --out Builds/LauncherRelease-93000 --build 93000 --version 0.93.0-review1 --notes "Race starts no longer jam on Dan's Backyard (grid rows 6 m apart); Abandoned Cabin Jump landing and run-out cleared of brush, resets there on open trail; acorn areas on the world map with directions, place names and a north arrow; minimap compass. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
