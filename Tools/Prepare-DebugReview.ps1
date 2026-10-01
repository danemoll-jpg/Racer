$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.63.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/62000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.63.0-review1
Mountain Reverse local terrain, pavement and natural barrier cleanup.
F3: Debug Mode. F4: screenshot and comment. F6: debug menu, fly, export ZIP and report folder.
Debug Mode suspends race timeouts. Fly inspection pauses the world and invalidates competitive race records.
See Review/VALIDATION.md for targeted checks and gameplay-review limitations.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/DebugReporting/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('VALIDATION.md','DEBUG_MODE.md')){Copy-Item -LiteralPath (Join-Path 'Docs/DebugReporting' $file) -Destination $review}
$mountain=Join-Path $runtime 'MountainCleanup'
New-Item -ItemType Directory -Force -Path $mountain | Out-Null
Copy-Item -LiteralPath 'Docs/MountainCleanup/VALIDATION.md' -Destination $mountain
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.63.0-review1-Windows --previous-catalog Builds/LauncherRelease-62000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-62000/assets/game-manifest.json --out Builds/LauncherRelease-63000 --build 63000 --version 0.63.0-review1 --notes "Mountain Reverse ten-area cleanup plus F3/F4 bug reporting, portable report ZIPs and detached debug fly. Debug Mode suspends race timeout; debug movement invalidates records. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
