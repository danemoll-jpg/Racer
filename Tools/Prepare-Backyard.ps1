$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.36.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-35000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-35000/assets/game-manifest.json `
 --out Builds/LauncherRelease-36000 --build 36000 --version 0.36.0-review1 `
 --notes "Dan's Backyard Forward and Reverse: property start, dump jump and reverse bypass, wooded gully course, optional ridge shortcut and South Cherokee return. Local Kyle driveway transition repair. Existing courses and accepted vehicle/recovery/AI tuning preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}
