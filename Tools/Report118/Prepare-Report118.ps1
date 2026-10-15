$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.118.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.118.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/117000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.118.0-review1
Leviathan: the two ROAD CLOSED barricades Dan called out are gone (Forward's in the field past the summit, Reverse's across the dirt way by the church); Kyle's hill is one even slope with a rounded top (race only); the arrow boards are gone, chevrons stay only at real road corners, and more gates guide the course instead (the next gate is in view from each one, a gate at every turn off a road); the Reverse lake out-and-back no longer gives a false WRONG WAY. Riders: closed hands round the grips and wheel, and five new hair styles (Mohawk, Crew cut, Spiky, Buzz, Curly), from Chastain Rush.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report118-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.118.0-review1-Windows --previous-catalog Builds/LauncherRelease-117000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-117000/assets/game-manifest.json --out Builds/LauncherRelease-118000 --build 118000 --version 0.118.0-review1 --notes "Leviathan: two barricades removed, Kyle's hill smoothed, gates instead of arrow boards, the Reverse lake out-and-back; riders' new hands and five new hair styles. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
