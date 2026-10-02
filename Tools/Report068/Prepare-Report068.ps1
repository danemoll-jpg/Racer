$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.68.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/67000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.68.0-review1
Reset now puts you on the nearest point of the track, facing the race direction, and always succeeds.
Nobody falls through the world: falling below it, quitting a race or starting Free Roam lands on solid ground.
Mountain Loop: Reverse junction/sign/crease/cap-gap fixes, Homeward Summit landing runout, earth berms after
four jumps, hidden holes under 0.67 seam covers made solid. Pause menu: END RACE / RETURN TO MENU.
Fewer ground arrows on every course (redundant ones removed; see Review/ARROWS.md).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every report disposition and targeted verification.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report068/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report068/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/Report068/ARROWS.md' -Destination $review
Copy-Item -LiteralPath 'Docs/Report068/BARRIERS.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.68.0-review1-Windows --previous-catalog Builds/LauncherRelease-67000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-67000/assets/game-manifest.json --out Builds/LauncherRelease-68000 --build 68000 --version 0.68.0-review1 --notes "Nearest-point reset that always succeeds, fall-through safety (failsafe, quit/Free Roam placement, start marker, solid seam holes), Mountain Reverse BUG-003..006 fixes, Homeward Summit landing runout, post-jump berms, pause-menu End Race label, redundant arrows removed. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
