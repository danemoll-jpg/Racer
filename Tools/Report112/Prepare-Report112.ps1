$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.112.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.112.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/111000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.112.0-review1
Campaign balancing round 1 (Chapter 1 and 4 rivals' pace), the Street Loop jump is now a parked trailer with its loading ramps down and its sign stands on posts, and Shop vehicles can be bought and chosen from the Race garage.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report112-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.112.0-review1-Windows --previous-catalog Builds/LauncherRelease-111000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-111000/assets/game-manifest.json --out Builds/LauncherRelease-112000 --build 112000 --version 0.112.0-review1 --notes "Campaign balancing round 1 (Chapter 1 and 4 rival pace), the trailer jump and its sign on Street Loop, buy and choose Shop vehicles from the Race garage. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
