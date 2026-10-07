$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.92.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.92.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/91000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.92.0-review1
Menus: one press of the D-pad or stick moves one row, top to bottom as drawn; the focused row has a white frame. A never changes a value: left / right do (held to repeat).
Vehicles are chosen in the garage view: campaign events open on the vehicle you last drove; split-screen players each choose in their own half.
Every chapter final awards a vehicle (chapter 2: the Pebble Coupe); the Grand Championship adds the champion's paint.
Split screen: AI rivals, time of day, weather and traffic.
Forest Loop Reverse: the bump at 1.5 km that threw bikes is smoothed.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report092-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.92.0-review1-Windows --previous-catalog Builds/LauncherRelease-91000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-91000/assets/game-manifest.json --out Builds/LauncherRelease-92000 --build 92000 --version 0.92.0-review1 --notes "Menus: one row per press, focus frame, values change with left / right only; vehicles chosen in the garage view (campaign defaults to the last driven); every chapter final awards a vehicle; split-screen stage 2 (AI rivals, conditions, traffic); Forest Loop Reverse bump smoothed. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
