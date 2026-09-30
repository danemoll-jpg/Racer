$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.52.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/51000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.52.0-review1
Race Setup > Tracks: Select/Confirm commits the highlighted course immediately.
Highlighting and Back never commit. Preview remains a separate optional action.
Maps, Records and Top 10 track browsing remain read-only.
All eight stable course/direction identities and existing records are preserved.
Existing physics, world geometry, AI, scoring, HUD and racing minimap preserved.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/UI/TrackSelection/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.52.0-review1-Windows --previous-catalog Builds/LauncherRelease-51000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-51000/assets/game-manifest.json --out Builds/LauncherRelease-52000 --build 52000 --version 0.52.0-review1 --notes "Fix Race Setup track selection: Select/Confirm commits the highlighted course. Optional map preview retained; Maps and Records browsing remain read-only. All eight course/direction identities preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
