$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.61.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/60000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.61.0-review1
Start driving in Free Roam; Start/Menu opens existing options. Supported Mountain roads and forward-facing Reverse Summit merge. Accepted Forest cave preserved.
Gold marks optional routes; teal follows the main course. Reverse Summit Traverse now merges facing the race direction.
Accepted Forest cave, Backyard courses, global vehicle settings and music preserved.
See Review/VALIDATION.md for targeted checks and gameplay-review limitations.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/MountainPolish/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('VALIDATION.md','MountainLoopReverse-after-s0.png','MountainLoopReverse-after-tail-overview.png')){Copy-Item -LiteralPath (Join-Path 'Docs/MountainPolish' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.61.0-review1-Windows --previous-catalog Builds/LauncherRelease-60000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-60000/assets/game-manifest.json --out Builds/LauncherRelease-61000 --build 61000 --version 0.61.0-review1 --notes "Free Roam startup and menu prompt, accurate results controls, local Forest tree grounding and supported Mountain corridors with natural Reverse Summit merge. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}








