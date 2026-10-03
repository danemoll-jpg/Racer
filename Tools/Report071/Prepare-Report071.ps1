$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.71.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/70000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.71.0-review1
New look ("Clear Day"): sun and shadows with more shape, a proper sky with haze that hides the edge of the world, grass / dirt / road read as different surfaces, lakes reflect the sky, gentle colour grading. Same courses, same handling.
Free Roam: the raised catch mound of the Summit Homeward Flight is gone. The jump lands on open natural ground with a clear run-out, flies further (moto ~244 m, ATV ~224 m) and still scores; the straight path up the mountain is open both ways.
Mountain Loop Forward, Climbing Ridge Cut: the hole and slab at the left edge are gone, cutting the corner across the grass at the entrance no longer flips you, and the hollow at the rejoin is filled.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md and Review/LOOK.md for every report disposition, the look and the frame-rate measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report071/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report071/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/Report071/LOOK.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.71.0-review1-Windows --previous-catalog Builds/LauncherRelease-70000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-70000/assets/game-manifest.json --out Builds/LauncherRelease-71000 --build 71000 --version 0.71.0-review1 --notes "Clear Day lighting and atmosphere; Free Roam Summit Homeward catch mound replaced by open natural ground (longer scored flights, straight mountain path open); Climbing Ridge Cut entrance slab/notch and corner-cut flips fixed, rejoin hollow filled. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
