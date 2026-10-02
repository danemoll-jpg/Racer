$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.64.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/63000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.64.0-review1
Mountain debug-report terrain and sign cleanup; working Debug menu input and ZIP export.
Race Complete: B / Escape returns directly to Main Menu from Results tabs and pages.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for individual report dispositions and targeted verification.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/ReportCleanup/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/ReportCleanup/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.64.0-review1-Windows --previous-catalog Builds/LauncherRelease-63000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-63000/assets/game-manifest.json --out Builds/LauncherRelease-64000 --build 64000 --version 0.64.0-review1 --notes "Mountain report cleanup, conservative worldwide sign audit, interactive Debug menu and ZIP export, Results B to Main Menu. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
