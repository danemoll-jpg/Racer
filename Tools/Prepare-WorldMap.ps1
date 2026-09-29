$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.44.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/43000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.44.0-review1
Tree-Top foliage pulled back from the approved main trail; dense slow bypass retained.
COMPLETE RACE appears after human finish while AI remain and uses the existing CR-064 estimates.
The approved main course is preserved. Both routes use existing gold minimap overlays, checkpoint entitlement and variable AI strategy.
Review/TRACK_MAP.png shows the actual implemented geometry. Full menu map uses actual saved world imagery; racing minimap unchanged. Reverse remains deferred.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/WorldMap/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/BackyardForward/TRACK_MAP.png' -Destination $review
Copy-Item -LiteralPath 'Docs/BackyardForward/ATLAS.md' -Destination $review
foreach($file in @('WORLD_MAP.png','README.md','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/WorldMap' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.44.0-review1-Windows --previous-catalog Builds/LauncherRelease-43000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-43000/assets/game-manifest.json --out Builds/LauncherRelease-44000 --build 44000 --version 0.44.0-review1 --notes "Tree-Top bush clearance; COMPLETE RACE reuses CR-064; rendered full-world menu map and coordinate planning atlas; racing minimap unchanged. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
