$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.98.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.98.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/97000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.98.0-review1
Police chase: being chased is Getaway only (Cop vs Runner is a cop role now), no track choice, a cop is already in pursuit when it starts, heat shown big with an alert at each rise, backups on the minimap, a police helicopter you can see and hear from heat 4, radio lines outside the panel.
Put back: the 0.96 road-closed barriers are gone except the one at Mountain Loop's Climbing Ridge Cut (now facing the driver, words fit); the Free Roam storm drain is open again as in 0.95 with its seams closed.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report098-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.98.0-review1-Windows --previous-catalog Builds/LauncherRelease-97000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-97000/assets/game-manifest.json --out Builds/LauncherRelease-98000 --build 98000 --version 0.98.0-review1 --notes "Police chase is Getaway only with a cop in pursuit from the start, big heat alerts, a visible helicopter and radio lines outside the panel; the 0.96 road-closed barriers removed (one kept, facing the driver); the Free Roam storm drain back to 0.95 with its seams closed. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
