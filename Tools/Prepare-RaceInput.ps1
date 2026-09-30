$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.53.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/52000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.53.0-review1
Map and pause/menu buttons remain responsive with throttle or steering held.
Only the consumed UI action must be released before another menu action.
Repeated opening/closing, held-button protection and disconnect recovery verified.
Race Setup track confirmation and read-only Maps/Records browsing preserved.
Existing physics, world geometry, AI, scoring, HUD and racing minimap preserved.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/UI/RaceInput/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.53.0-review1-Windows --previous-catalog Builds/LauncherRelease-52000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-52000/assets/game-manifest.json --out Builds/LauncherRelease-53000 --build 53000 --version 0.53.0-review1 --notes "Fix in-race map and menu input locking while throttle or steering is held. Release barriers track only the consumed UI action, preserving held-button protection and effective bindings. Countdown and repeated pause/map use verified. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
