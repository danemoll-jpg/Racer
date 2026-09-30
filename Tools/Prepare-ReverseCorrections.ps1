$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.47.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/46000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.47.0-review1
Dan's Backyard Loop - Reverse: focused route surface, divergence, approach and collision corrections.
Consistent main-route dirt, three Reverse-only timber closures, two straightened ramp approaches and three repaired terrain joins.
Review/ATLAS.html offers separately selectable Forward and Reverse views of the actual saved geometry.
The approved Forward routes and global vehicle/AI settings remain unchanged. Reverse has no optional shortcuts.
The full menu map retains the permanent physical network; the racing minimap uses the new actual route.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/BackyardReverseCorrections/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('ATLAS.html','ATLAS.md','Forward.png','Reverse.png')){Copy-Item -LiteralPath (Join-Path 'Docs/BackyardReverse' $file) -Destination $review}
foreach($file in @('WORLD_MAP.png','README.md','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/WorldMap' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.47.0-review1-Windows --previous-catalog Builds/LauncherRelease-46000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-46000/assets/game-manifest.json --out Builds/LauncherRelease-47000 --build 47000 --version 0.47.0-review1 --notes "Focused Backyard Reverse corrections: consistent main-route dirt, three race-only timber closures, straight ramp approaches and three local collision repairs. Forward and global vehicle/AI settings preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}



