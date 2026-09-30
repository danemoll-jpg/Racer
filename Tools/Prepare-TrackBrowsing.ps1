$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.50.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/49000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.50.0-review1
Browse tracks directly in Records and World Map during Free Roam or between races.
Active races keep these screens on the current course.
Preview each course map before choosing Use This Track in Race Setup.
Saved record counts and configuration labels make existing times easier to find.
Existing physics, world geometry, AI, scoring, HUD and racing minimap preserved.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/UI/TrackBrowsing/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.50.0-review1-Windows --previous-catalog Builds/LauncherRelease-49000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-49000/assets/game-manifest.json --out Builds/LauncherRelease-50000 --build 50000 --version 0.50.0-review1 --notes "Track browsing in Records and World Map outside active races; Race Setup route previews. Existing records and race settings preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
