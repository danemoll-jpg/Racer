$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.33.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-32000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-32000/assets/game-manifest.json `
 --out Builds/LauncherRelease-33000 --build 33000 --version 0.33.0-review1 `
 --notes "Local racing mini-map, Pine Ridge one-way exit, McFadden Cut physical sign, beige property paving and recorded coyotes. Recovery and AI probabilities unchanged. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}





