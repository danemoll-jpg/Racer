$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.43.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/42000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.43.0-review1
Dan's Backyard Loop - Forward: optional Tree-Top Trail and Abandoned Cabin Jump.
The approved main course is preserved. Both routes use existing gold minimap overlays, checkpoint entitlement and variable AI strategy.
Review/TRACK_MAP.png shows the actual implemented geometry. Reverse and full-world/menu-map upgrades remain deferred.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/ShortcutRevision/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/BackyardForward/TRACK_MAP.png' -Destination $review
Copy-Item -LiteralPath 'Docs/BackyardForward/ATLAS.md' -Destination $review
foreach($file in @('tree-top.png','cabin.png','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/ShortcutRevision' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.43.0-review1-Windows --previous-catalog Builds/LauncherRelease-42000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-42000/assets/game-manifest.json --out Builds/LauncherRelease-43000 --build 43000 --version 0.43.0-review1 --notes "Two optional Backyard Forward forest shortcuts: Tree-Top Trail and Abandoned Cabin Jump; protected main course; existing checkpoint/minimap/AI integration; updated visual map. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
