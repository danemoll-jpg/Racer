$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.104.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.104.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/103000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.104.0-review1
American spelling everywhere you read (Color, Gray, canceled...). Hwy 92 looks like the real road today: stores, a grocery, gas stations, fast food, banks and offices with their parking lots, entrances, sidewalks, street lights, signs and traffic lights, in every scene; Granite Creek Cut stays open; the plot across from S Cherokee Ln is left empty for the stunt course.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report104-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.104.0-review1-Windows --previous-catalog Builds/LauncherRelease-103000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-103000/assets/game-manifest.json --out Builds/LauncherRelease-104000 --build 104000 --version 0.104.0-review1 --notes "American spelling in the game; Hwy 92 roadside like the real road today (every scene). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
