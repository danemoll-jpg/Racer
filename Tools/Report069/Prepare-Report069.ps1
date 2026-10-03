$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.69.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/68000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.69.0-review1
Mountain Loop: road edges are smooth and flush, so you can ride off and straight back on (sawtooth outline gone).
Earth berms at the Reverse summit crest bend and at the Forward Climbing Ridge Cut rejoin.
Grass removed from the road at the Street Loop start; torn/ribbed Mountain terrain at two places smoothed.
Street Loop signs: FENCE LINE SMASH and ANDERSON'S removed; LAKE SHORE sign lifted out of the hillside.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every report disposition and targeted verification.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report069/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report069/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/Report069/BARRIERS.md' -Destination $review
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.69.0-review1-Windows --previous-catalog Builds/LauncherRelease-68000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-68000/assets/game-manifest.json --out Builds/LauncherRelease-69000 --build 69000 --version 0.69.0-review1 --notes "Smooth flush Mountain road edges (sawtooth outline straightened, flush collidable shoulders), berms at the Reverse summit crest bend and Forward Climbing Ridge Cut rejoin, Street Loop asphalt colour restored, BUG-006/007 terrain smoothed, two signs removed and one reseated. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
