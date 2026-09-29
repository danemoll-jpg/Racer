$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.38.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-37000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-37000/assets/game-manifest.json `
 --out Builds/LauncherRelease-38000 --build 38000 --version 0.38.0-review1 `
 --notes "Dan's Backyard Forward: nine hard anchors, corrected dump launch, physical dump depression, one substantial ravine with two crossings, narrow wooded terrain trail and additional jumps. Accepted property and roads preserved. Reverse and optional shortcuts deferred. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Publisher preparation failed"}
