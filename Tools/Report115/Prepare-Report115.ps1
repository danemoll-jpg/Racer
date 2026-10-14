$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.115.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.115.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/114000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.115.0-review1
New course: Leviathan, Forward and Reverse (Race > Tracks): the Street Loop and the Mountain Loop joined at the lake, over the summit jump every lap. The combined-course proposal markers are gone from Free Roam.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report115-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.115.0-review1-Windows --previous-catalog Builds/LauncherRelease-114000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-114000/assets/game-manifest.json --out Builds/LauncherRelease-115000 --build 115000 --version 0.115.0-review1 --notes "New course Leviathan (Forward and Reverse): Street Loop + Mountain Loop with the summit jump; the Free Roam proposal markers removed. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
