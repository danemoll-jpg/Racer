$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.34.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-33000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-33000/assets/game-manifest.json `
 --out Builds/LauncherRelease-34000 --build 34000 --version 0.34.0-review1 `
 --notes "Correct property driveways and Rocky Way Acres sign; F3 world XYZ and F4 clipboard; Dan supplied coyote recording. Recovery and AI probabilities unchanged. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}






