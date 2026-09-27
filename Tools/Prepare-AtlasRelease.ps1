$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.30.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-29000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-29000/assets/game-manifest.json `
 --out Builds/LauncherRelease-30000 --build 30000 --version 0.30.0-review1 `
 --notes "Granite Saddle promoted to Forest Reverse main route; six-course visual navigation, route atlas and local House 3 driveway cleanup. Laurel protected; no replacement shortcut or physics/AI/recovery redesign. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}



