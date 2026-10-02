$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.67.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/66000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.67.0-review1
20-report cleanup: Mountain Loop (both directions) floating roads, edge gaps, terrain poking through the
pavement, South Face receiving-deck hillside and lower-route tunnel, signs, rocks and arrows; Street Loop cairn;
Dan's Backyard arrows and run-up heading. Debug sessions now survive quitting until exported.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every report disposition and targeted verification.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report067/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report067/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.67.0-review1-Windows --previous-catalog Builds/LauncherRelease-66000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-66000/assets/game-manifest.json --out Builds/LauncherRelease-67000 --build 67000 --version 0.67.0-review1 --notes "20-report cleanup (Mountain Loop supports, gaps, pavement intrusions, South Face hillside/tunnel, signs, rocks, arrows; Street Loop cairn; Backyard arrows/run-up) and persistent Debug sessions. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
