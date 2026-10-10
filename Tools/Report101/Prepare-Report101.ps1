$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.101.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.101.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/100000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.101.0-review1
Abandoned Cabin Jump (Dan's Backyard Forward and Free Roam): one straight run-up from the main road to the ramp with lead-in posts and chevrons, the entry crest smoothed, the ramp and roof lip half as wide again, and the brush trimmed so a decent jump clears it (a reset from the brush puts you just past it).
Dan's house: the garage/kennel turned so its doors face the parking area, the parking area paved up to the doors, the driveway beside it removed, a small sidewalk, chain-link fence around the yard; the spikes in the ground by the garage are gone.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report101-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.101.0-review1-Windows --previous-catalog Builds/LauncherRelease-100000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-100000/assets/game-manifest.json --out Builds/LauncherRelease-101000 --build 101000 --version 0.101.0-review1 --notes "Abandoned Cabin Jump: straight run-up, wider ramp and lip, brush trimmed so a decent jump clears it (race and Free Roam); spikes by the garage removed; Dan's garage turned to the parking area with sidewalk and chain-link fence. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
