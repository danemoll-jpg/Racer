$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.100.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.100.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/99000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.100.0-review1
Free look while driving: the right stick looks around (R3 held looks behind; on the keyboard hold the right mouse button and move the mouse). The moon is drawn in the driving view on clear nights (a new game's Free Roam starts near a full moon). Hidden police see you going either way.
Fixed: the Mountain Loop trench beside the road is filled from verge to verge, vehicle pictures in menus are never squished, and the Free Roam storm drain has a solid top you can drive across.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report100-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.100.0-review1-Windows --previous-catalog Builds/LauncherRelease-99000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-99000/assets/game-manifest.json --out Builds/LauncherRelease-100000 --build 100000 --version 0.100.0-review1 --notes "Free look while driving (right stick, R3, right mouse button); the moon visible on clear nights; hidden police see both ways; Mountain Loop trench filled; vehicle pictures never squished; storm drain top solid. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
