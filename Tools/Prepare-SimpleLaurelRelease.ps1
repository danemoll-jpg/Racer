$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.27.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-26000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-26000/assets/game-manifest.json `
 --out Builds/LauncherRelease-27000 --build 27000 --version 0.27.0-review1 `
 --notes "Laurel Pass: remove recent curved shortcut strip and raised terminal ramp; shape one straight launch ramp into the original near-side ground to clear the chasm. Far-side road, normal Street Loop routes, recovery and vehicle physics preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}
