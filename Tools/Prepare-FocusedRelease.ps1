$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.28.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-27000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-27000/assets/game-manifest.json `
 --out Builds/LauncherRelease-28000 --build 28000 --version 0.28.0-review1 `
 --notes "Laurel Pass: straighten launch along incoming heading. Restore safe recovery on flat shared ramp surfaces and relocate obsolete Laurel exclusion. AI recovers after sustained lack of route/rejoin progress. Other roads and vehicle physics preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}

