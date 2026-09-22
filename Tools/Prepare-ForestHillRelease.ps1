$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.29.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-28000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-28000/assets/game-manifest.json `
 --out Builds/LauncherRelease-29000 --build 29000 --version 0.29.0-review1 `
 --notes "Forest Loop Reverse: replace the abrupt opening trail face with a gradual uphill transition on the existing terrain. Route alignment, other tracks, vehicle physics, recovery and AI preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}


