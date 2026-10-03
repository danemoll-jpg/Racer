$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.72.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/71000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.72.0-review1
Time of day and weather (look and sound only; handling, AI and records are unchanged): Race Setup has Time of Day (Day / Dusk / Night) and Weather (Clear / Rain / Snow), kept for the whole race and remembered. Night has headlights on every vehicle, glowing arrows and gates, stars and a moon. Rain falls around you with wet, shiny roads and rain sound; snow falls and covers the ground. Nothing falls inside caves or tunnels.
Free Roam: a live day-night cycle (1 real minute = 1 game hour, starting at 08:00, clock in the Free Roam text), headlights come on by themselves when it gets dark; Weather on the Free Roam page.
Free Roam: the torn dirt trail at the hairpin below the Summit Homeward run-out (and the pale spiky bank beside it) is one clean trail again.
Mountain Loop: rival riders no longer vanish after the big jumps (they were being reset in mid-air), take the jumps reliably in both directions, and a rival that does fail is put back where it failed.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md, Review/AI_JUMPS.md and Review/LOOK.md for every report disposition, the AI jump results, the looks and the frame-rate measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report072/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report072/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/Report072/LOOK.md' -Destination $review
Copy-Item -LiteralPath 'Docs/Report072/AI_JUMPS.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.72.0-review1-Windows --previous-catalog Builds/LauncherRelease-71000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-71000/assets/game-manifest.json --out Builds/LauncherRelease-72000 --build 72000 --version 0.72.0-review1 --notes "Clear Day lighting and atmosphere; Free Roam Summit Homeward catch mound replaced by open natural ground (longer scored flights, straight mountain path open); Climbing Ridge Cut entrance slab/notch and corner-cut flips fixed, rejoin hollow filled. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
