$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.95.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.95.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/93000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.95.0-review1
Names: Settings > Gameplay "Player name" (on-screen keyboard) and "Name tags"; names on the Top 10; split-screen players have names and their times count on the Top 10.
Rivals: the campaign's named cast in every race; a starting-grid card, the driver ahead / behind with the gap, names in results and the winner shot.
Split screen: Mode: Race / Free Roam / Police Chase. Free Roam for two (player 2 can be the AI); each player changes their own camera view (X on their controller, V on the keyboard).
Police Chase: one player is the cop in the patrol car (RB / H: siren), the other runs; the bust meter fills when the cop is close and the runner slow; then the roles swap.
Abandoned Cabin Jump (Dan's Backyard Forward): the shrubs are back as far as a well-hit jump clears; a reset from inside them puts you just past them.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report095-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.95.0-review1-Windows --previous-catalog Builds/LauncherRelease-93000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-93000/assets/game-manifest.json --out Builds/LauncherRelease-94000 --build 94000 --version 0.95.0-review1 --notes "Player names on the Top 10, rivals and name tags; split-screen Free Roam for two with each player's own camera views; Police Chase (cop vs runner) with a patrol car; Abandoned Cabin Jump shrubs back part-way. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
