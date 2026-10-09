$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.99.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.99.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/98000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.99.0-review1
Hidden police in Free Roam (speed past a parked patrol car and a Getaway chase starts; Settings > Gameplay turns it off), a police motorcycle, the patrol car and the police bike unlock for races through police goals, and the police radio speaks with voice clips (SourceArt/Audio/PoliceRadio).
Fixed: the Ridge Cut road-closed sign's words are one-sided and hidden by terrain, the two Mountain Loop roadside trenches are filled, and AI cars no longer brake into the summit jump's lip.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report099-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.99.0-review1-Windows --previous-catalog Builds/LauncherRelease-98000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-98000/assets/game-manifest.json --out Builds/LauncherRelease-99000 --build 99000 --version 0.99.0-review1 --notes "Hidden police in Free Roam, a police motorcycle, police vehicles unlocked by police goals, police radio voices; Ridge Cut sign text one-sided, two Mountain Loop roadside trenches filled, AI cars clear the summit jump. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
