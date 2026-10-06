$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.83.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.83.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/82000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.83.0-review1
Mountain Loop Forward: the summit hairpin no longer throws riders off (tilted sliver faces in the deck made flat), and the broken strip across the road after the inner corner is closed.
The people in the scripted scenes: Dan (black T-shirt with a ring-and-bird chest print, jeans), Kyle (black leather jacket, jeans) and the brother.
Tracks: the course map is always beside the list and shows the highlighted course's route.
Loading screens show the game poster.
Winner shot and view changes: the rider's face and hair are always there (no bald head in any camera).
Records: a plain Top 10 per track (all vehicles, all race lengths); left / right changes track; empty boards say why.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report083-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.83.0-review1-Windows --previous-catalog Builds/LauncherRelease-82000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-82000/assets/game-manifest.json --out Builds/LauncherRelease-83000 --build 83000 --version 0.83.0-review1 --notes "Scene characters (Dan, Kyle, brother), always-on track map, poster loading screen, winner head fix, plain Top 10 records, two Mountain Loop Forward fixes. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
