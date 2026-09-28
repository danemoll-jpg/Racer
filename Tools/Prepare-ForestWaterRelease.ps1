$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.31.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-30000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-30000/assets/game-manifest.json `
 --out Builds/LauncherRelease-31000 --build 31000 --version 0.31.0-review1 `
 --notes "Current Forest main preserved; existing House 3 Detour restored optional, straight pool/lake jump and orphan driveway cleanup. Updated atlas and visual navigation; Laurel and vehicle physics preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}




