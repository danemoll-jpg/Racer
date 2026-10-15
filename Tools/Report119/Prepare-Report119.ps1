$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.119.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.119.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/118000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.119.0-review1
Leviathan: the barricade across the way to CP 17 (Forward) and the fence at the church dirt way's other end (Reverse) are gone; Forward CP 18 is on Kyle's drive, straight ahead from the lake path. The night sky: stars stay behind hills, trees and buildings. New: Stunt Races (Race > Race type: stunt vehicles only on the Mountain Loop and Leviathan, a trick bonus or split score, rivals doing tricks, their own records) and the campaign's Stunt chapter (after chapter 4: seven events, the Clay Hopper Cup as its prize).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report119-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.119.0-review1-Windows --previous-catalog Builds/LauncherRelease-118000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-118000/assets/game-manifest.json --out Builds/LauncherRelease-119000 --build 119000 --version 0.119.0-review1 --notes "Leviathan: Dan's 0.118 reports (two closures out, CP 18 on Kyle's drive); night stars behind the hills; Stunt Races and the campaign's Stunt chapter. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
