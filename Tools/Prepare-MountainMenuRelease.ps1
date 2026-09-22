$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.24.1-review1-Windows `
 --previous-catalog Builds/MountainMenu-PublicPrevious/update-catalog.json `
 --previous-manifest Builds/MountainMenu-PublicPrevious/game-manifest.json `
 --out Builds/LauncherRelease-24001 --build 24001 --version 0.24.1-review1 `
 --notes "Restore Mountain Loop Forward and Reverse menu launches by including both existing Mountain scenes in the Windows build. No track geometry changes. Both menu selections and race startups verified. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}
