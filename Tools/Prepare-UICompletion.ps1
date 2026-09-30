$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.49.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/48000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.49.0-review1
World Map, Playlists, Records and Results: Phases 3 through 5 together.
Controller map selection/travel, list-based playlist drafts and readable record tables.
Independent best-lap/race achievements, lap times and championship summaries.
Accepted Phase 1-2 UI foundation and core menus retained.
Existing physics, world geometry, AI, scoring, HUD and racing minimap preserved.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/UI/Phase345/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.49.0-review1-Windows --previous-catalog Builds/LauncherRelease-48000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-48000/assets/game-manifest.json --out Builds/LauncherRelease-49000 --build 49000 --version 0.49.0-review1 --notes "Remaining approved UI phases together: controller World Map, transactional playlist editor, Records tables and finish/results/championship presentation. Accepted Phase 1-2 and protected gameplay retained. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
