$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.37.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-36000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-36000/assets/game-manifest.json `
 --out Builds/LauncherRelease-37000 --build 37000 --version 0.37.0-review1 `
 --notes "Rejected Backyard course removed; forest restored; Kyle fix preserved; Dan driveway, flat parking, fence support and mailbox corrected; eight non-colliding anchor validation markers. No new forest trail. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Publisher preparation failed"}
