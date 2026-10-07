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
CAMPAIGN (main menu, first entry): chapter 1, Street Loop, six events. Earn money, buy vehicles in the Shop, unlock courses.
Race offers the courses and vehicles the campaign has opened; Free Roam has the whole world but only your vehicles.
Settings > Gameplay > Unlock everything (testing) opens every course and vehicle (the campaign save is not written).
Remaining flickering vehicle surfaces fixed (grilles, wheels, bar pads, traffic bodies).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report090-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.90.0-review1-Windows --previous-catalog Builds/LauncherRelease-89000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-89000/assets/game-manifest.json --out Builds/LauncherRelease-90000 --build 90000 --version 0.90.0-review1 --notes "Campaign round 1: campaign save and screen, locking, money and vehicle shop, the Street Loop chapter; remaining flickering vehicle surfaces fixed. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
