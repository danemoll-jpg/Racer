$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.106.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.106.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/105000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.106.0-review1
Red Clay Stunt Park on the Hwy 92 lot across from S Cherokee Ln: kickers from small to huge, a bus gap, a junk-car gap, quarter-pipes and a half-pipe, a two-level deck, a loop and a barrel-roll ramp; flips, rolls, spins and combos scored in Free Roam; the 10-minute Stunt event (solo or split-screen, medals, Top 10); two stunt vehicles you steer in the air (the Clay Hopper buggy, and the Skywhip 250 motorcycle for a Gold).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report106-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.106.0-review1-Windows --previous-catalog Builds/LauncherRelease-105000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-105000/assets/game-manifest.json --out Builds/LauncherRelease-106000 --build 106000 --version 0.106.0-review1 --notes "Red Clay Stunt Park on the Hwy 92 lot (kickers, gaps, quarter-pipes, half-pipe, two-level deck, loop, barrel-roll ramp); stunt scoring with flips, rolls, spins and combos; the 10-minute Stunt event with medals and a Top 10; the Clay Hopper stunt buggy and the Skywhip 250 stunt motorcycle with air steering. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
