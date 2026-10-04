$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.75.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.75.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/74000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.75.0-review1
New models for the whole garage, made in Blender like the 0.73 motorcycle: the Trail Four ATV, the Street Classic coupe and the Longroof GT wagon (cars with a cabin, glass and the driver at the wheel). Handling, colliders, wheel positions and camera are unchanged. Garage > Model: Classic / New now switches every vehicle (default New, remembered); the AI follow it. Ambient traffic keeps the classic cars.
Rider customization: Garage > Rider... - body (man / woman), skin tone, hair style and colour, hat (none, flat cap, baseball cap, beanie, cowboy hat) and colour, shirt (T-shirt, long sleeve, jacket) and colour, pants (jeans, shorts) and colour, with a live preview and Randomize. Left / right changes a row. Remembered; the default is the familiar rider. Every AI gets a random rider each race. With Model: Classic the old rider is shown.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every item and the frame-rate measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report075/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report075/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report075-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.75.0-review1-Windows --previous-catalog Builds/LauncherRelease-74000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-74000/assets/game-manifest.json --out Builds/LauncherRelease-75000 --build 75000 --version 0.75.0-review1 --notes "New Blender models for the whole garage (Trail Four ATV, Street Classic, Longroof GT) with Model: Classic / New for every vehicle; rider customization on a new garage Rider page (body, skin, hair, hat, shirt, pants and colours, Randomize); random AI riders. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
