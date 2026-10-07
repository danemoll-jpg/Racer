$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.90.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.90.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/89000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.90.0-review1
CAMPAIGN: chapters 2-4 (Forest Loop, Dan's Backyard, Mountain Loop), four championships, upgrades in the Shop, an ending.
Locked vehicles are silhouettes with a padlock; locked tracks are greyed with how to reach them.
New players get a welcome panel, a controls card and short hints (Settings > Gameplay > Hints).
SPLIT SCREEN (main menu): two players, one race; player 2 joins with Start or Enter, or set Player 2: AI driver.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report090-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.90.0-review1-Windows --previous-catalog Builds/LauncherRelease-89000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-89000/assets/game-manifest.json --out Builds/LauncherRelease-90000 --build 90000 --version 0.90.0-review1 --notes "Locked-vehicle silhouettes, new-player hints, campaign round 2 (chapters 2-4, championships, upgrades), split-screen stage 1. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
