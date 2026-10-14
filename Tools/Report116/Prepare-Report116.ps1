$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.116.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.116.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/115000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.116.0-review1
Leviathan shows where to turn: only the next checkpoint gate is shown, arrow and chevron boards at every turn off the road, the next checkpoint on the HUD and the course ahead on the minimap; the maps show only Leviathan's own route. Leviathan Reverse has two shortcuts of its own, Hollow Creek Hop and Rocky Way Cut (each can be switched off in the F6 debug menu, Debug Mode).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report116-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.116.0-review1-Windows --previous-catalog Builds/LauncherRelease-115000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-115000/assets/game-manifest.json --out Builds/LauncherRelease-116000 --build 116000 --version 0.116.0-review1 --notes "Leviathan: turn signs, only the next gate shown, next checkpoint on the HUD, course ahead on the minimap, maps show only its route; Leviathan Reverse: two new shortcuts (Hollow Creek Hop, Rocky Way Cut). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
