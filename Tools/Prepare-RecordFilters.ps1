$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.51.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/50000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.51.0-review1
Records now shows overall fastest compatible times with compact Filters.
Lap Top 10 combines every race length; Race Top 10 retains the selected lap count.
All Vehicles is the default; filter by saved profile or select a historical rules era.
Existing records and historical categories are preserved without migration.
Existing physics, world geometry, AI, scoring, HUD and racing minimap preserved.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/UI/RecordFilters/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.51.0-review1-Windows --previous-catalog Builds/LauncherRelease-50000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-50000/assets/game-manifest.json --out Builds/LauncherRelease-51000 --build 51000 --version 0.51.0-review1 --notes "Records Filters: fastest compatible laps across race lengths and vehicles; race totals retain lap-count selection. Historical rules and existing saves preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
