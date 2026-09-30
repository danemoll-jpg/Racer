$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.45.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/44000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.45.0-review1
Temporary Backyard markers removed; reference coordinates retained for future Reverse.
Explicit bound controller/keyboard Complete Race hints reuse the existing CR-064 behavior.
Corrected property landmarks, wooded roadside/property margins, and a shared permanent map network.
Review/TRACK_MAP.png shows the actual implemented geometry. Full menu map shows permanent roads/trails/shortcuts with an optional current course overlay; racing minimap unchanged. Reverse is upcoming.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/WorldCleanup/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/BackyardForward/TRACK_MAP.png' -Destination $review
Copy-Item -LiteralPath 'Docs/BackyardForward/ATLAS.md' -Destination $review
foreach($file in @('WORLD_MAP.png','README.md','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/WorldMap' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.45.0-review1-Windows --previous-catalog Builds/LauncherRelease-44000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-44000/assets/game-manifest.json --out Builds/LauncherRelease-45000 --build 45000 --version 0.45.0-review1 --notes "World cleanup: documented anchors, property landmarks, woodland density, permanent map network and bound Complete Race prompts. Western Gullies audited only; Backyard Reverse upcoming. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
