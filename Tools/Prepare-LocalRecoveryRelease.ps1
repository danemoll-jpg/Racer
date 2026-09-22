$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.25.0-review1-Windows `
 --previous-catalog Builds/LocalRecovery-PublicPrevious/update-catalog.json `
 --previous-manifest Builds/LocalRecovery-PublicPrevious/game-manifest.json `
 --out Builds/LauncherRelease-25000 --build 25000 --version 0.25.0-review1 `
 --notes "Local Laurel Pass replacement with straight main-road jump; localized Mountain Reverse first-arrow surface correction; frequent stable occupied recovery anchors and short reset delay preserve completed jumps. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}
