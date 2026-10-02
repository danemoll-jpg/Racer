$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.65.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/64000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.65.0-review1
Debug export closes the session; next capture starts a fresh BUG-001 with history preserved.
START NEW DEBUG SESSION closes without export, with confirmation for unexported reports.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for focused lifecycle and input verification.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/DebugLifecycle/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugLifecycle/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.65.0-review1-Windows --previous-catalog Builds/LauncherRelease-64000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-64000/assets/game-manifest.json --out Builds/LauncherRelease-65000 --build 65000 --version 0.65.0-review1 --notes "Debug session closure on export, fresh BUG-001 numbering, retained history, explicit restart confirmation and session ID/count display. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
