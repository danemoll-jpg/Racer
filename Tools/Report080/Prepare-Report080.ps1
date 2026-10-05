$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.80.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.80.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/79000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.80.0-review1
Clean-up from the 0.79 review: the marker posts standing on roads in Free Roam are gone; Hwy 92's lane paint is one clean layout at Trickum Rd and S Cherokee Ln (edge lines, stop lines); the giant-jump landing and the summit roads are mountain dirt; in Mountain Loop Reverse the hole beside the crest outcrop is filled, the edge notches at the start of the Downhill Ridge Cut are smoothed, and road decks with open air under their edges stand on earth banks.
Fist wave on the controller is now LB (keyboard F unchanged). In Trailer Mode LB keeps its 0.25x hold and RB its 0.5x toggle; use F there.
Cars on every course: both cars can race (and be raced by AI) on all eight courses; motorcycle and ATV unchanged; records stay per vehicle.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every item and the measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report080/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report080/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report080-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.80.0-review1-Windows --previous-catalog Builds/LauncherRelease-79000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-79000/assets/game-manifest.json --out Builds/LauncherRelease-80000 --build 80000 --version 0.80.0-review1 --notes "0.79 clean-up (posts off roads, Hwy 92 junction paint, mountain dirt look, Mountain Loop Reverse hole / bump / floating decks), fist wave on LB, cars allowed on every course. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
