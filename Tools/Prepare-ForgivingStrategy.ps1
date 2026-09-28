$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.32.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-31000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-31000/assets/game-manifest.json `
 --out Builds/LauncherRelease-32000 --build 32000 --version 0.32.0-review1 `
 --notes "Forgiving earned recovery with newest-safe selection and independent situational AI shortcut decisions. Main favored in neutral conditions; no physics or geometry changes. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}





