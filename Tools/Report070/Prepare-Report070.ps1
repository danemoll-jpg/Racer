$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.70.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/69000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.70.0-review1
Quit Game now asks first: "Quit Woodstock Rush?" [Cancel] [Quit] (Cancel is selected; B / Esc cancel).
Mountain Loop Reverse: a tall rock outcrop at the summit crest bend stops full-speed riders flying off; the Summit Traverse entrance stays open.
Mountain campsite sits on the ground in the Mountain races; Free Roam has a new "Campsite" map landmark.
Climbing Ridge Cut entrance no longer flips bikes and ATVs (hidden faces under the junction removed).
Smoother ground: Street Loop cliff beside the highway, highway right verge, and the Climbing Ridge Cut rejoin.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every report disposition and targeted verification.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report070/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report070/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/Report070/BARRIERS.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.70.0-review1-Windows --previous-catalog Builds/LauncherRelease-69000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-69000/assets/game-manifest.json --out Builds/LauncherRelease-70000 --build 70000 --version 0.70.0-review1 --notes "Quit confirmation, 12 m rock outcrop at the Reverse summit crest bend, Mountain campsite seated and Campsite landmark, Climbing Ridge Cut entry hidden faces removed, Street Loop terrain and verge smoothed, Climbing Ridge Cut rejoin gores smoothed. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
