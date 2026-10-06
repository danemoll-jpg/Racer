$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.82.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.82.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/81000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.82.0-review1
Loading screens: every load (first start, a course, Free Roam, back to the menu) now shows a loading screen with what is loading, the conditions and vehicle, the course map, a real progress bar and a tip, instead of a frozen picture.
Smoother: the stutter in Free Roam traffic and at race starts is fixed (road lookups, traffic bookkeeping and sound building no longer stall frames); Free Roam loads about twice as fast.
Hwy 92 junction lines at S Cherokee Ln and Trickum Rd now stay on the asphalt.
Garage: five bars compare Top speed, Acceleration, Grip, Handling and Weight / contact across all ten vehicles.
Winning: the camera shows the winner's celebration for 2.5 s before the results (A / Space skips).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report082-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.82.0-review1-Windows --previous-catalog Builds/LauncherRelease-81000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-81000/assets/game-manifest.json --out Builds/LauncherRelease-82000 --build 82000 --version 0.82.0-review1 --notes "Loading screens, stutter fixes (Free Roam traffic, race starts, faster loads), Hwy 92 junction lines on the asphalt, garage stat bars, winner celebration camera. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
