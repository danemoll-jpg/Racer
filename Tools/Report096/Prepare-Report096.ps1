$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.96.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.96.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/95000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.96.0-review1
Campaign events: a jump, speed trap or smash event shows its own panel (medal targets, this run's best, time left), a result banner for every attempt and a clear COMPLETE line; medals are drawn everywhere instead of written. Each chapter starts with a practice lap before its first race, with new practice laps on the reverse courses.
Free Roam: the cracks in the ground along the gullies are closed; the storm-drain box beside the trail is sunk and covered. Mountain Loop: the roadside holes are filled; roads that leave the route are closed in the race scenes; the AI cars clear the summit jump.
Police: AI cops chase you. Police Chase > Game: Getaway (every human runs, the cops are AI; heat, roadblocks, an escape meter, radio calls, a Top 10), and the Runner role against an AI cop now works (Difficulty Easy / Normal / Hard).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report096-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.96.0-review1-Windows --previous-catalog Builds/LauncherRelease-95000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-95000/assets/game-manifest.json --out Builds/LauncherRelease-96000 --build 96000 --version 0.96.0-review1 --notes "Campaign event panel, attempt banners and drawn medals; a practice lap before each chapter race; Free Roam ground cracks and the storm-drain box fixed; Mountain Loop roadside holes filled and off-route roads closed; AI cops (Getaway, Runner vs AI cop). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
