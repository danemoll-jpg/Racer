$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.41.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/40000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.41.0-review1
Dan's Backyard Loop - Forward and its dense garbage dump are APPROVED.
Review/TRACK_MAP.png is a high-resolution overhead render of the actual approved course for shortcut planning. No shortcuts or Reverse course were built.
The track selector has standardized alphabetical names and unassigned difficulty labels.
After finishing with AI still racing, SIMULATE REMAINING RACERS uses the existing CR-064 classification system.
Play-Racer.cmd uses the existing signed launcher/updater. General menu redesign is backlog only.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/TrackUi/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/BackyardForward/TRACK_MAP.png' -Destination $review
Copy-Item -LiteralPath 'Docs/BackyardForward/ATLAS.md' -Destination $review
foreach($file in @('track-selector.png','waiting.png','results.png','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/TrackUi' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.41.0-review1-Windows --previous-catalog Builds/LauncherRelease-40000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-40000/assets/game-manifest.json --out Builds/LauncherRelease-41000 --build 41000 --version 0.41.0-review1 --notes "Approved Backyard visual shortcut-planning map; standardized alphabetical track names with TBD difficulty; direct post-finish Simulate Remaining Racers using unchanged CR-064. Course geometry and saved record identities preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
