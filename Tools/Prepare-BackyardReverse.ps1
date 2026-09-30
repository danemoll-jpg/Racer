$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.46.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/45000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.46.0-review1
Dan's Backyard Loop - Reverse: distinct gully crossings, wooded dump bypass and pool-house flight.
Shared-world fixes: ramp trees, permanent supported McFadden water, Roger's house, grounded trees and physical sign occlusion.
Review/ATLAS.html offers separately selectable Forward and Reverse views of the actual saved geometry.
The approved Forward routes and global vehicle/AI settings remain unchanged. Reverse has no optional shortcuts.
The full menu map retains the permanent physical network; the racing minimap uses the new actual route.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/BackyardReverse/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('ATLAS.html','ATLAS.md','Forward.png','Reverse.png')){Copy-Item -LiteralPath (Join-Path 'Docs/BackyardReverse' $file) -Destination $review}
foreach($file in @('WORLD_MAP.png','README.md','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/WorldMap' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.46.0-review1-Windows --previous-catalog Builds/LauncherRelease-45000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-45000/assets/game-manifest.json --out Builds/LauncherRelease-46000 --build 46000 --version 0.46.0-review1 --notes "Shared-world corrections and Dan's Backyard Loop - Reverse main course. Approved Forward and global vehicle/AI settings preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}

