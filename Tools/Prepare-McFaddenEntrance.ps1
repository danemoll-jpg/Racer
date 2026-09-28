$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.35.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-34000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-34000/assets/game-manifest.json `
 --out Builds/LauncherRelease-35000 --build 35000 --version 0.35.0-review1 `
 --notes "Move existing Rocky Way Acres sign to current McFadden driveway entrance; trim driveway visual overlap at main-road edge. Accepted systems and collision surfaces preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}







