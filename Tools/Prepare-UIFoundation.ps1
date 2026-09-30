$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.48.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/47000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.48.0-review1
Controller-first shared UI foundation and core menus: Phases 1 and 2 together.
Main Menu, Race Setup, Garage, Tracks, Opponents, Pause, Settings, Music and controller folder selection.
Reusable text entry, graphical prompts, caller-aware Back and input ownership.
World Map, Playlists, Records and Results remain available for their later redesign phases.
Existing physics, world geometry, AI, scoring, HUD and racing minimap preserved.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/UI/Phase12/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.48.0-review1-Windows --previous-catalog Builds/LauncherRelease-47000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-47000/assets/game-manifest.json --out Builds/LauncherRelease-48000 --build 48000 --version 0.48.0-review1 --notes "Controller-first UI foundation and core menus, Phases 1 and 2 together. Caller-aware navigation, graphical prompts, settings/music, controller folder picker and reusable text entry. Later-phase screens and protected gameplay retained. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
