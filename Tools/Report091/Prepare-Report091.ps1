$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.91.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.91.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/90000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.91.0-review1
Controller: every menu row is reachable in order (CAMPAIGN and SPLIT SCREEN included); B goes back one level.
SPLIT SCREEN: whoever opens it is player 1 on their device; each player's row changes their device; A / Start on another controller or Enter on the keyboard joins; Player 2 is the AI until someone joins.
CAMPAIGN jump events: a jump counts if you land it on your wheels; jump as often as you like until the time runs out, the best counts; reset takes you back to the run-up; Pause > End event keeps the best.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report091-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.91.0-review1-Windows --previous-catalog Builds/LauncherRelease-90000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-90000/assets/game-manifest.json --out Builds/LauncherRelease-91000 --build 91000 --version 0.91.0-review1 --notes "Controller reaches every menu row (Campaign, Split Screen); a controller can be split-screen player 1; campaign jumps count when you survive them (several attempts, best counts). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
