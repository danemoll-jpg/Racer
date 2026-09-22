$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.24.0-review1-Windows `
 --previous-catalog Builds/CR133-PublicPrevious/update-catalog.json `
 --previous-manifest Builds/CR133-PublicPrevious/game-manifest.json `
 --out Builds/LauncherRelease-24000 --build 24000 --version 0.24.0-review1 `
 --notes 'Corrective release: straight continuous Laurel runway and ramp; rebuilt mountain junctions and connected road surfaces; terrain-integrated road support in both Mountain Loop directions; earned post-jump recovery progress. Vehicle handling and Highway 92 preserved. Targeted structural checks and Windows build passed. Source commit 32f9220b06372496d6830967d2bd51946a72f616.'
if($LASTEXITCODE -ne 0){throw "Existing publisher preparation failed with exit code $LASTEXITCODE"}
