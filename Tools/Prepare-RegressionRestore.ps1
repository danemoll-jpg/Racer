$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.66.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/65000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.66.0-review1
Mountain Loop - Reverse regression repair: lower main route reopened and South Face Summit jump restored to 0.63 known-good behavior.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for root causes, known-good comparison and targeted verification.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/RegressionRestore/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/RegressionRestore/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.66.0-review1-Windows --previous-catalog Builds/LauncherRelease-65000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-65000/assets/game-manifest.json --out Builds/LauncherRelease-66000 --build 66000 --version 0.66.0-review1 --notes "Mountain Reverse regression repair: lower main route clearance and South Face Summit jump restored to 0.63 known-good behavior. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
