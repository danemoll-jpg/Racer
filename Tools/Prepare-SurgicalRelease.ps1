$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.26.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-25000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-25000/assets/game-manifest.json `
 --out Builds/LauncherRelease-26000 --build 26000 --version 0.26.0-review1 `
 --notes "Restore Laurel Pass main-road terrain from pre-shortcut Git history; separate straight shortcut ramp onto the existing road; correct faceted collision beneath the second Mountain Reverse arrow. Recovery/reset and vehicle physics unchanged. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}
