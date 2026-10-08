$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.95.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.95.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/94000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.95.0-review1
Police Chase: main menu > POLICE CHASE (also in Free Roam's menu). One player: you are the cop on the full screen and keep the role; two players: split-screen, the roles swap. Game: Speed Patrol: clock speeders with the radar, lights on (RB / H) to chase, fill the pull-over meter; points, a Top 10 per round length.
Names: only in the campaign and, in split-screen, for the other player. Settings > Gameplay "Name tags: On / Off".
Free Roam: the lake has water again (shallow, drivable out, ice in Snow); holes at the edge of the world sealed; no resetting back into pits; floating trees grounded.
Acorns: a banner for every acorn found (count and area); a panel for the 24th (the Turf Rocket unlock), shown once if you missed it.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report095-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.95.0-review1-Windows --previous-catalog Builds/LauncherRelease-94000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-94000/assets/game-manifest.json --out Builds/LauncherRelease-95000 --build 95000 --version 0.95.0-review1 --notes "Police Chase on the main menu and in Free Roam (solo full screen, roles swap only with two players) and Speed Patrol; names only in the campaign and for the other split-screen player; the Free Roam lake; world-edge holes sealed and pit-proof resets; acorn pickup banner and unlock panel. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
