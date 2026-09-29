$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.38.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/37000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@"
Woodstock Rush 0.38.0-review1 - Dan's Backyard Forward

Forward is implemented and awaiting Dan's gameplay review. Select Dan's Backyard - Forward from Courses. Nine hard anchors, a terrain dump jump, one extended ravine with two crossings, narrow wooded trail, and four additional terrain crests. Existing property and flat parking are preserved. Reverse and optional shortcuts remain deferred.

Review/Forward.svg and Review/ATLAS.md describe the actual saved course. Play-Racer.cmd uses the existing signed launcher/updater. Music and saved settings remain separate. F3 toggles WORLD XYZ; F4 copies the current location.
"@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/BackyardForward/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('Forward.svg','ATLAS.md','start.png','parking.png','dump.png','big-gully.png','same-ravine.png','second-crossing.png','woods.png','return.png','terrain-profiles.csv')){Copy-Item -LiteralPath (Join-Path 'Docs/BackyardForward' $file) -Destination (Join-Path $review $file)}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.38.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-37000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-37000/assets/game-manifest.json `
 --out Builds/LauncherRelease-38000 --build 38000 --version 0.38.0-review1 `
 --notes "Dan's Backyard Forward: nine hard anchors, corrected dump launch, physical dump depression, one substantial ravine with two crossings, narrow wooded terrain trail and additional jumps. Accepted property and roads preserved. Reverse and optional shortcuts deferred. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Publisher preparation failed"}
