$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.88.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.88.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/87000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.88.0-review1
Kyle's place: one straight gravel driveway from the street down to the garage doors, grass where the old drives were, more trees, a flat front lawn two steps below the porch, and a mailbox.
Car floorboards no longer flash (all six cars); the Pebble Coupe has a full roof over its rear seats.
Find all 24 Woodland Acorns to unlock the Turf Rocket riding mower.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report088-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.88.0-review1-Windows --previous-catalog Builds/LauncherRelease-87000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-87000/assets/game-manifest.json --out Builds/LauncherRelease-88000 --build 88000 --version 0.88.0-review1 --notes "Kyle's driveway, yard and mailbox; car floorboard flashing fixed and Pebble Coupe roof; acorn reward riding mower. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
