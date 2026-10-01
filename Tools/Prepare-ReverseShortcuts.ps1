$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.54.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/53000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.54.0-review1
Two optional player-only Backyard Reverse shortcuts: Abandoned Logging Ridge and Storm Drain / Gully Jump.
Forward closes the new drain grate and logging gate; free roam opens them.
Gold marks optional routes and dashed gold marks the underground drain beneath existing surface trails.
Accepted main routes, original Forward shortcuts, global vehicle/AI settings, HUD, racing minimap and music preserved.
See Review/ATLAS.html and VALIDATION.md for exact anchors, technical checks and gameplay-review limitations.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/ReverseShortcuts/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('ATLAS.html','Forward.png','Reverse.png','REQUEST.md','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/ReverseShortcuts' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.54.0-review1-Windows --previous-catalog Builds/LauncherRelease-53000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-53000/assets/game-manifest.json --out Builds/LauncherRelease-54000 --build 54000 --version 0.54.0-review1 --notes "Backyard Reverse Logging Ridge and underground Storm Drain / Gully Jump. Forward gates and free-roam state, protected original roads, gold/dashed route atlas. Player-only shortcuts; detailed timing and handling await Dan review. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
